using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto model);
    Task<AuthResult> LoginAsync(LoginDto model);
    Task<AuthResult> RefreshTokenAsync(string token);
}

public interface ICsvImportService
{
    Task<ImportResult> ImportCsvAsync(Stream csvStream, string fileName, int accountId, string userId);
    Task<List<CsvPreviewRow>> PreviewCsvAsync(Stream csvStream, string fileName);
}

public interface ITransactionService
{
    Task<List<Transaction>> GetTransactionsAsync(string userId, int? accountId = null, int? householdId = null);
    Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto, string userId);
    Task<Transaction> UpdateTransactionAsync(int id, UpdateTransactionDto dto, string userId);
    Task<bool> DeleteTransactionAsync(int id, string userId);
    Task ApplyCategoryRulesAsync(int transactionId);
}

public interface IHouseholdService
{
    Task<Household> CreateHouseholdAsync(CreateHouseholdDto dto, string userId);
    Task<List<Household>> GetUserHouseholdsAsync(string userId);
    Task<Household> GetHouseholdAsync(int id, string userId);
    Task<bool> AddMemberAsync(int householdId, string email, string userId);
    Task<bool> RemoveMemberAsync(int householdId, string memberUserId, string userId);
    Task<List<Settlement>> CalculateSettlementsAsync(int householdId);
}

public interface IPsd2Service
{
    Task<List<BankConnectionDto>> GetUserBankConnectionsAsync(string userId);
    Task<BankConnectionResultDto> CreateBankConnectionAsync(CreateBankConnectionDto dto, string userId);
    Task<bool> CompleteBankConnectionAsync(string connectionId, string authorizationCode, string userId);
    Task<bool> DisconnectBankAsync(int connectionId, string userId);
    Task<List<ExternalAccountDto>> GetExternalAccountsAsync(int connectionId, string userId);
    Task<List<ExternalTransactionDto>> GetExternalTransactionsAsync(int externalAccountId, DateTime? fromDate, DateTime? toDate, string userId);
    Task<ImportResult> ImportExternalTransactionsAsync(ImportExternalTransactionsDto dto, string userId);
    Task<bool> SyncBankConnectionAsync(int connectionId, string userId);
    Task<bool> LinkAccountAsync(LinkAccountDto dto, string userId);
    Task<bool> CheckConsentExpiryAsync(string userId);
}

public interface IPsd2EventLogService
{
    Task LogEventAsync(string userId, string eventType, string description, int? bankConnectionId = null, 
        object? eventData = null, bool isSuccess = true, string? errorMessage = null, 
        string? ipAddress = null, string? userAgent = null);
    Task<List<Psd2EventLogDto>> GetEventLogsAsync(string userId, int? bankConnectionId = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
    Task<List<Psd2EventLogDto>> GetSystemEventLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, 
        string? eventType = null, int page = 1, int pageSize = 50);
}