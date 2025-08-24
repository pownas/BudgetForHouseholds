using Microsoft.EntityFrameworkCore;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.Services;

namespace BudgetApp.Api.BackgroundServices;

/// <summary>
/// Background service for automatic synchronization of PSD2 bank connections
/// Runs periodically to sync active bank connections and check for consent expiry
/// </summary>
public class Psd2SyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Psd2SyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(6); // Sync every 6 hours
    private readonly TimeSpan _consentCheckInterval = TimeSpan.FromHours(24); // Check consent daily

    public Psd2SyncBackgroundService(IServiceProvider serviceProvider, ILogger<Psd2SyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PSD2 Sync Background Service started");

        var lastConsentCheck = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BudgetAppDbContext>();
                var psd2Service = scope.ServiceProvider.GetRequiredService<IPsd2Service>();
                var eventLogService = scope.ServiceProvider.GetRequiredService<IPsd2EventLogService>();

                // Get all active connections that need syncing
                var connectionsToSync = await context.BankConnections
                    .Where(bc => bc.Status == ConnectionStatus.Active && 
                                 (bc.LastSyncAt == null || bc.LastSyncAt < DateTime.UtcNow.Subtract(_syncInterval)))
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("Found {Count} bank connections to sync", connectionsToSync.Count);

                // Sync each connection
                foreach (var connection in connectionsToSync)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        _logger.LogDebug("Syncing bank connection {ConnectionId} for user {UserId}", 
                            connection.Id, connection.UserId);

                        var success = await psd2Service.SyncBankConnectionAsync(connection.Id, connection.UserId);
                        
                        if (success)
                        {
                            _logger.LogDebug("Successfully synced bank connection {ConnectionId}", connection.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to sync bank connection {ConnectionId}", connection.Id);
                        }

                        // Add a small delay between syncs to avoid overwhelming the aggregator
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing bank connection {ConnectionId}", connection.Id);
                        
                        await eventLogService.LogEventAsync(connection.UserId, Psd2EventTypes.SyncFailed,
                            "Automatic sync failed", connection.Id, null, false, ex.Message);
                    }
                }

                // Check consent expiry (daily)
                if (DateTime.UtcNow - lastConsentCheck >= _consentCheckInterval)
                {
                    _logger.LogInformation("Checking consent expiry for all users");

                    var usersWithConnections = await context.BankConnections
                        .Where(bc => bc.Status == ConnectionStatus.Active)
                        .Select(bc => bc.UserId)
                        .Distinct()
                        .ToListAsync(stoppingToken);

                    foreach (var userId in usersWithConnections)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            await psd2Service.CheckConsentExpiryAsync(userId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error checking consent expiry for user {UserId}", userId);
                        }
                    }

                    lastConsentCheck = DateTime.UtcNow;
                }

                _logger.LogDebug("PSD2 sync cycle completed. Next sync in {Interval}", _syncInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PSD2 sync background service");
            }

            // Wait for the next sync interval
            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("PSD2 Sync Background Service stopped");
    }
}