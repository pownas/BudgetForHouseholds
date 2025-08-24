using System.ComponentModel.DataAnnotations;
using BudgetApp.Api.Models;

namespace BudgetApp.Api.DTOs;

// Auth DTOs
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }
}

// Transaction DTOs
public class CreateTransactionDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Counterpart { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
    public SharingStatus SharingStatus { get; set; } = SharingStatus.Private;
    public List<TransactionSplitDto>? Splits { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateTransactionDto
{
    public DateTime? Date { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? Counterpart { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
    public SharingStatus? SharingStatus { get; set; }
    public List<TransactionSplitDto>? Splits { get; set; }
    public List<string>? Tags { get; set; }
}

public class TransactionSplitDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(0.0001, 1.0)]
    public decimal Percentage { get; set; }

    public string? Notes { get; set; }
}

// Household DTOs
public class CreateHouseholdDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

// Account DTOs
public class CreateAccountDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; } = AccountType.BankAccount;
    public SharingType SharingType { get; set; } = SharingType.Private;
    public int? HouseholdId { get; set; }
    public decimal CurrentBalance { get; set; } = 0;
    public string? Description { get; set; }
}

// Budget DTOs
public class CreateBudgetDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public BudgetScope Scope { get; set; } = BudgetScope.Individual;
    public int? HouseholdId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
}

// Import DTOs
public class ImportResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
}

public class CsvPreviewRow
{
    public int RowNumber { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}

// Category DTOs
public class CreateCategoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public CategoryScope Scope { get; set; } = CategoryScope.User;
    public int? HouseholdId { get; set; }
    public string Color { get; set; } = "#000000";
    public string? Icon { get; set; }
}

// Rule DTOs
public class CreateCategoryRuleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public string? DescriptionContains { get; set; }
    public string? CounterpartContains { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool AutoCategorize { get; set; } = true;
    public bool AutoSplit { get; set; } = false;
    public string? AutoSplitConfig { get; set; }
    public string? AutoTags { get; set; }
    public int Priority { get; set; } = 0;
    public int? HouseholdId { get; set; }
}

// PSD2/Open Banking DTOs
public class BankConnectionDto
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankId { get; set; } = string.Empty;
    public ConnectionStatus Status { get; set; }
    public DateTime ConsentGivenAt { get; set; }
    public DateTime? ConsentExpiresAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int AccountCount { get; set; }
}

public class CreateBankConnectionDto
{
    [Required]
    public string BankId { get; set; } = string.Empty;
    
    [Required]
    public string RedirectUrl { get; set; } = string.Empty;
}

public class BankConnectionResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? ConnectionId { get; set; }
}

public class ExternalAccountDto
{
    public int Id { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public ExternalAccountType AccountType { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
    public string Currency { get; set; } = "SEK";
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
    public int? LinkedAccountId { get; set; }
    public string? LinkedAccountName { get; set; }
}

public class ExternalTransactionDto
{
    public int Id { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Counterpart { get; set; }
    public string? Reference { get; set; }
    public string Currency { get; set; } = "SEK";
    public bool IsImported { get; set; }
}

public class ImportExternalTransactionsDto
{
    [Required]
    public int ExternalAccountId { get; set; }
    
    [Required]
    public int AccountId { get; set; }
    
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class SyncBankConnectionDto
{
    [Required]
    public int ConnectionId { get; set; }
}

public class LinkAccountDto
{
    [Required]
    public int ExternalAccountId { get; set; }
    
    [Required]
    public int AccountId { get; set; }
}

// PSD2 Event Log DTOs
public class Psd2EventLogDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public string? EventData { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? BankConnectionId { get; set; }
    public string? BankName { get; set; }
}