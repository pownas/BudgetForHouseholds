using Microsoft.EntityFrameworkCore;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public class Psd2Service : IPsd2Service
{
    private readonly BudgetAppDbContext _context;
    private readonly ILogger<Psd2Service> _logger;

    public Psd2Service(BudgetAppDbContext context, ILogger<Psd2Service> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<BankConnectionDto>> GetUserBankConnectionsAsync(string userId)
    {
        var connections = await _context.BankConnections
            .Where(bc => bc.UserId == userId)
            .Include(bc => bc.ExternalAccounts)
            .OrderByDescending(bc => bc.CreatedAt)
            .ToListAsync();

        return connections.Select(bc => new BankConnectionDto
        {
            Id = bc.Id,
            BankName = bc.BankName,
            BankId = bc.BankId,
            Status = bc.Status,
            ConsentGivenAt = bc.ConsentGivenAt,
            ConsentExpiresAt = bc.ConsentExpiresAt,
            LastSyncAt = bc.LastSyncAt,
            ErrorMessage = bc.ErrorMessage,
            AccountCount = bc.ExternalAccounts.Count
        }).ToList();
    }

    public async Task<BankConnectionResultDto> CreateBankConnectionAsync(CreateBankConnectionDto dto, string userId)
    {
        try
        {
            // In a real implementation, this would call the aggregator API (e.g., Tink, Nordigen)
            // For now, we'll create a mock implementation
            
            var bankName = GetBankNameFromId(dto.BankId);
            var externalConnectionId = Guid.NewGuid().ToString();
            
            var connection = new BankConnection
            {
                UserId = userId,
                BankName = bankName,
                BankId = dto.BankId,
                ExternalConnectionId = externalConnectionId,
                Status = ConnectionStatus.Pending,
                ConsentGivenAt = DateTime.UtcNow,
                ConsentExpiresAt = DateTime.UtcNow.AddDays(90) // PSD2 standard: 90 days max
            };

            _context.BankConnections.Add(connection);
            await _context.SaveChangesAsync();

            // In real implementation, this would be the actual authorization URL from the aggregator
            var authUrl = $"https://mock-aggregator.com/auth?connection_id={externalConnectionId}&redirect_uri={Uri.EscapeDataString(dto.RedirectUrl)}";

            _logger.LogInformation("Created bank connection {ConnectionId} for user {UserId} with bank {BankId}", 
                connection.Id, userId, dto.BankId);

            return new BankConnectionResultDto
            {
                Success = true,
                AuthorizationUrl = authUrl,
                ConnectionId = externalConnectionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create bank connection for user {UserId} with bank {BankId}", userId, dto.BankId);
            return new BankConnectionResultDto
            {
                Success = false,
                Error = "Failed to initiate bank connection"
            };
        }
    }

    public async Task<bool> CompleteBankConnectionAsync(string connectionId, string authorizationCode, string userId)
    {
        try
        {
            var connection = await _context.BankConnections
                .FirstOrDefaultAsync(bc => bc.ExternalConnectionId == connectionId && bc.UserId == userId);

            if (connection == null)
            {
                _logger.LogWarning("Bank connection {ConnectionId} not found for user {UserId}", connectionId, userId);
                return false;
            }

            // In real implementation, this would exchange the authorization code for access tokens
            // and fetch the user's accounts
            await CreateMockAccountsForConnection(connection);

            connection.Status = ConnectionStatus.Active;
            connection.LastSyncAt = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return false;
        }
    }

    public async Task<bool> DisconnectBankAsync(int connectionId, string userId)
    {
        try
        {
            var connection = await _context.BankConnections
                .Include(bc => bc.ExternalAccounts)
                .FirstOrDefaultAsync(bc => bc.Id == connectionId && bc.UserId == userId);

            if (connection == null)
            {
                return false;
            }

            // In real implementation, this would revoke the consent with the aggregator
            connection.Status = ConnectionStatus.Disconnected;
            connection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Disconnected bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return false;
        }
    }

    public async Task<List<ExternalAccountDto>> GetExternalAccountsAsync(int connectionId, string userId)
    {
        var connection = await _context.BankConnections
            .Include(bc => bc.ExternalAccounts)
            .ThenInclude(ea => ea.LinkedAccount)
            .FirstOrDefaultAsync(bc => bc.Id == connectionId && bc.UserId == userId);

        if (connection == null)
        {
            return new List<ExternalAccountDto>();
        }

        return connection.ExternalAccounts.Select(ea => new ExternalAccountDto
        {
            Id = ea.Id,
            ExternalAccountId = ea.ExternalAccountId,
            AccountName = ea.AccountName,
            AccountNumber = ea.AccountNumber,
            AccountType = ea.AccountType,
            CurrentBalance = ea.CurrentBalance,
            AvailableBalance = ea.AvailableBalance,
            Currency = ea.Currency,
            IsActive = ea.IsActive,
            LastUpdated = ea.LastUpdated,
            LinkedAccountId = ea.LinkedAccountId,
            LinkedAccountName = ea.LinkedAccount?.Name
        }).ToList();
    }

    public async Task<List<ExternalTransactionDto>> GetExternalTransactionsAsync(int externalAccountId, DateTime? fromDate, DateTime? toDate, string userId)
    {
        // Verify the user owns this external account
        var externalAccount = await _context.ExternalAccounts
            .Include(ea => ea.BankConnection)
            .FirstOrDefaultAsync(ea => ea.Id == externalAccountId && ea.BankConnection.UserId == userId);

        if (externalAccount == null)
        {
            return new List<ExternalTransactionDto>();
        }

        var query = _context.ExternalTransactions
            .Where(et => et.ExternalAccountId == externalAccountId);

        if (fromDate.HasValue)
        {
            query = query.Where(et => et.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(et => et.Date <= toDate.Value);
        }

        var transactions = await query
            .OrderByDescending(et => et.Date)
            .ToListAsync();

        return transactions.Select(et => new ExternalTransactionDto
        {
            Id = et.Id,
            ExternalTransactionId = et.ExternalTransactionId,
            Date = et.Date,
            Amount = et.Amount,
            Description = et.Description,
            Counterpart = et.Counterpart,
            Reference = et.Reference,
            Currency = et.Currency,
            IsImported = et.IsImported
        }).ToList();
    }

    public async Task<ImportResult> ImportExternalTransactionsAsync(ImportExternalTransactionsDto dto, string userId)
    {
        try
        {
            // Verify user owns both accounts
            var externalAccount = await _context.ExternalAccounts
                .Include(ea => ea.BankConnection)
                .FirstOrDefaultAsync(ea => ea.Id == dto.ExternalAccountId && ea.BankConnection.UserId == userId);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.OwnerId == userId);

            if (externalAccount == null || account == null)
            {
                return new ImportResult { Success = false, Error = "Account not found or access denied" };
            }

            var query = _context.ExternalTransactions
                .Where(et => et.ExternalAccountId == dto.ExternalAccountId && !et.IsImported);

            if (dto.FromDate.HasValue)
            {
                query = query.Where(et => et.Date >= dto.FromDate.Value);
            }

            if (dto.ToDate.HasValue)
            {
                query = query.Where(et => et.Date <= dto.ToDate.Value);
            }

            var externalTransactions = await query.ToListAsync();
            int importedCount = 0;
            int skippedCount = 0;

            foreach (var extTrans in externalTransactions)
            {
                // Check for duplicates
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.AccountId == dto.AccountId && 
                                           t.Date.Date == extTrans.Date.Date &&
                                           t.Amount == extTrans.Amount &&
                                           t.Description == extTrans.Description);

                if (existingTransaction != null)
                {
                    skippedCount++;
                    continue;
                }

                // Create new transaction
                var transaction = new Transaction
                {
                    AccountId = dto.AccountId,
                    Date = extTrans.Date,
                    Amount = extTrans.Amount,
                    Description = extTrans.Description,
                    Counterpart = extTrans.Counterpart,
                    CreatedAt = DateTime.UtcNow,
                    SharingStatus = SharingStatus.Private,
                    OwnerId = userId,
                    ExternalReference = extTrans.ExternalTransactionId
                };

                _context.Transactions.Add(transaction);
                extTrans.IsImported = true;
                extTrans.ImportedTransaction = transaction;
                importedCount++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imported {ImportedCount} transactions, skipped {SkippedCount} duplicates for user {UserId}", 
                importedCount, skippedCount, userId);

            return new ImportResult
            {
                Success = true,
                Message = $"Successfully imported {importedCount} transactions",
                ImportedCount = importedCount,
                SkippedCount = skippedCount,
                ErrorCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import external transactions for user {UserId}", userId);
            return new ImportResult
            {
                Success = false,
                Error = "Failed to import transactions"
            };
        }
    }

    public async Task<bool> SyncBankConnectionAsync(int connectionId, string userId)
    {
        try
        {
            var connection = await _context.BankConnections
                .Include(bc => bc.ExternalAccounts)
                .FirstOrDefaultAsync(bc => bc.Id == connectionId && bc.UserId == userId);

            if (connection == null || connection.Status != ConnectionStatus.Active)
            {
                return false;
            }

            // In real implementation, this would fetch fresh data from the aggregator
            await CreateMockTransactionsForConnection(connection);

            connection.LastSyncAt = DateTime.UtcNow;
            connection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Synced bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync bank connection {ConnectionId} for user {UserId}", connectionId, userId);
            return false;
        }
    }

    public async Task<bool> LinkAccountAsync(LinkAccountDto dto, string userId)
    {
        try
        {
            var externalAccount = await _context.ExternalAccounts
                .Include(ea => ea.BankConnection)
                .FirstOrDefaultAsync(ea => ea.Id == dto.ExternalAccountId && ea.BankConnection.UserId == userId);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.OwnerId == userId);

            if (externalAccount == null || account == null)
            {
                return false;
            }

            externalAccount.LinkedAccountId = dto.AccountId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Linked external account {ExternalAccountId} to account {AccountId} for user {UserId}", 
                dto.ExternalAccountId, dto.AccountId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link accounts for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CheckConsentExpiryAsync(string userId)
    {
        try
        {
            var connections = await _context.BankConnections
                .Where(bc => bc.UserId == userId && bc.Status == ConnectionStatus.Active)
                .ToListAsync();

            bool hasExpiring = false;
            var now = DateTime.UtcNow;
            var warningThreshold = now.AddDays(7); // Warn 7 days before expiry

            foreach (var connection in connections)
            {
                if (connection.ConsentExpiresAt.HasValue)
                {
                    if (connection.ConsentExpiresAt.Value <= now)
                    {
                        connection.Status = ConnectionStatus.ConsentExpired;
                        hasExpiring = true;
                    }
                    else if (connection.ConsentExpiresAt.Value <= warningThreshold)
                    {
                        hasExpiring = true;
                    }
                }
            }

            if (hasExpiring)
            {
                await _context.SaveChangesAsync();
            }

            return hasExpiring;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check consent expiry for user {UserId}", userId);
            return false;
        }
    }

    private async Task CreateMockAccountsForConnection(BankConnection connection)
    {
        // Create mock external accounts for demonstration
        var accounts = new List<ExternalAccount>
        {
            new ExternalAccount
            {
                BankConnectionId = connection.Id,
                ExternalAccountId = Guid.NewGuid().ToString(),
                AccountName = "Salary Account",
                AccountNumber = "****1234",
                AccountType = ExternalAccountType.Current,
                CurrentBalance = 15750.50m,
                AvailableBalance = 15750.50m,
                Currency = "SEK",
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            },
            new ExternalAccount
            {
                BankConnectionId = connection.Id,
                ExternalAccountId = Guid.NewGuid().ToString(),
                AccountName = "Savings Account",
                AccountNumber = "****5678",
                AccountType = ExternalAccountType.Savings,
                CurrentBalance = 45230.00m,
                Currency = "SEK",
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            }
        };

        _context.ExternalAccounts.AddRange(accounts);
        await _context.SaveChangesAsync();
    }

    private async Task CreateMockTransactionsForConnection(BankConnection connection)
    {
        var accounts = await _context.ExternalAccounts
            .Where(ea => ea.BankConnectionId == connection.Id)
            .ToListAsync();

        var random = new Random();
        var now = DateTime.UtcNow;

        foreach (var account in accounts)
        {
            // Create a few mock transactions
            var transactions = new List<ExternalTransaction>();
            
            for (int i = 0; i < 5; i++)
            {
                transactions.Add(new ExternalTransaction
                {
                    ExternalAccountId = account.Id,
                    ExternalTransactionId = Guid.NewGuid().ToString(),
                    Date = now.AddDays(-random.Next(1, 30)),
                    Amount = random.Next(-500, 100),
                    Description = GetRandomTransactionDescription(),
                    Counterpart = GetRandomCounterpart(),
                    Reference = $"REF{random.Next(100000, 999999)}",
                    Currency = "SEK",
                    IsImported = false
                });
            }

            _context.ExternalTransactions.AddRange(transactions);
        }

        await _context.SaveChangesAsync();
    }

    private string GetBankNameFromId(string bankId)
    {
        return bankId switch
        {
            "SWEDSESS" => "Swedbank",
            "HANDSESS" => "Handelsbanken",
            "SBABSESS" => "SEB",
            "NORDSESS" => "Nordea",
            _ => "Unknown Bank"
        };
    }

    private string GetRandomTransactionDescription()
    {
        var descriptions = new[]
        {
            "ICA Supermarket",
            "Spotify Premium",
            "SL Access",
            "Salary deposit",
            "Gas station payment",
            "Restaurant bill",
            "Online purchase",
            "Cash withdrawal"
        };
        var random = new Random();
        return descriptions[random.Next(descriptions.Length)];
    }

    private string GetRandomCounterpart()
    {
        var counterparts = new[]
        {
            "ICA AB",
            "Spotify Technology S.A.",
            "Storstockholms Lokaltrafik",
            "ABC Company Ltd",
            "Circle K",
            "Restaurant XYZ",
            "Webshop Inc",
            "ATM Network"
        };
        var random = new Random();
        return counterparts[random.Next(counterparts.Length)];
    }
}