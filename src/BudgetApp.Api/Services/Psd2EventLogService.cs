using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public class Psd2EventLogService : IPsd2EventLogService
{
    private readonly BudgetAppDbContext _context;
    private readonly ILogger<Psd2EventLogService> _logger;

    public Psd2EventLogService(BudgetAppDbContext context, ILogger<Psd2EventLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogEventAsync(string userId, string eventType, string description, 
        int? bankConnectionId = null, object? eventData = null, bool isSuccess = true, 
        string? errorMessage = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var eventLog = new Psd2EventLog
            {
                UserId = userId,
                BankConnectionId = bankConnectionId,
                EventType = eventType,
                EventDescription = description,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.Psd2EventLogs.Add(eventLog);
            await _context.SaveChangesAsync();

            // Log critical events to system logger as well
            if (!isSuccess || eventType == Psd2EventTypes.ConsentExpired)
            {
                _logger.LogWarning("PSD2 Event: {EventType} - {Description} for user {UserId}. Success: {IsSuccess}. Error: {ErrorMessage}",
                    eventType, description, userId, isSuccess, errorMessage);
            }
            else
            {
                _logger.LogInformation("PSD2 Event: {EventType} - {Description} for user {UserId}",
                    eventType, description, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log PSD2 event {EventType} for user {UserId}", eventType, userId);
            // Don't throw - event logging failures shouldn't break the main operation
        }
    }

    public async Task<List<Psd2EventLogDto>> GetEventLogsAsync(string userId, int? bankConnectionId = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        var query = _context.Psd2EventLogs
            .Include(el => el.BankConnection)
            .Where(el => el.UserId == userId);

        if (bankConnectionId.HasValue)
        {
            query = query.Where(el => el.BankConnectionId == bankConnectionId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(el => el.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(el => el.Timestamp <= toDate.Value);
        }

        var eventLogs = await query
            .OrderByDescending(el => el.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(el => new Psd2EventLogDto
            {
                Id = el.Id,
                EventType = el.EventType,
                EventDescription = el.EventDescription,
                EventData = el.EventData,
                Timestamp = el.Timestamp,
                IsSuccess = el.IsSuccess,
                ErrorMessage = el.ErrorMessage,
                BankConnectionId = el.BankConnectionId,
                BankName = el.BankConnection != null ? el.BankConnection.BankName : null
            })
            .ToListAsync();

        return eventLogs;
    }

    public async Task<List<Psd2EventLogDto>> GetSystemEventLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, 
        string? eventType = null, int page = 1, int pageSize = 50)
    {
        var query = _context.Psd2EventLogs
            .Include(el => el.BankConnection)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(el => el.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(el => el.Timestamp <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(el => el.EventType == eventType);
        }

        var eventLogs = await query
            .OrderByDescending(el => el.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(el => new Psd2EventLogDto
            {
                Id = el.Id,
                EventType = el.EventType,
                EventDescription = el.EventDescription,
                EventData = el.EventData,
                Timestamp = el.Timestamp,
                IsSuccess = el.IsSuccess,
                ErrorMessage = el.ErrorMessage,
                BankConnectionId = el.BankConnectionId,
                BankName = el.BankConnection != null ? el.BankConnection.BankName : null
            })
            .ToListAsync();

        return eventLogs;
    }
}