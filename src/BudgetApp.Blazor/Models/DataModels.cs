namespace BudgetApp.Blazor.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public int? HouseholdId { get; set; }
    public SharingType SharingType { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum AccountType
{
    BankAccount = 0,
    CreditCard = 1,
    Cash = 2,
    SavingsAccount = 3,
    Debt = 4
}

public enum SharingType
{
    Private = 0,
    SharedInHousehold = 1
}

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Counterpart { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
    public SharingStatus SharingStatus { get; set; }
    public string? ExternalId { get; set; }
    public string? ImportHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Account? Account { get; set; }
    public Category? Category { get; set; }
    public List<TransactionSplit>? Splits { get; set; }
    public List<TransactionTag>? Tags { get; set; }
}

public enum SharingStatus
{
    Private = 0,
    SharedInHousehold = 1
}

public class TransactionSplit
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public User? User { get; set; }
}

public class TransactionTag
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string Tag { get; set; } = string.Empty;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public CategoryScope Scope { get; set; }
    public string? UserId { get; set; }
    public int? HouseholdId { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum CategoryScope
{
    User = 0,
    Household = 1,
    System = 2
}

public class Household
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<HouseholdMember>? Members { get; set; }
}

public class HouseholdMember
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public HouseholdRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public User? User { get; set; }
}

public enum HouseholdRole
{
    Member = 0,
    Administrator = 1
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public SharingType SharingType { get; set; }
    public int? HouseholdId { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Description { get; set; }
}

public class CreateTransactionDto
{
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Counterpart { get; set; }
    public int? CategoryId { get; set; }
    public string? Notes { get; set; }
    public SharingStatus SharingStatus { get; set; }
    public List<TransactionSplitDto>? Splits { get; set; }
    public List<string>? Tags { get; set; }
}

public class TransactionSplitDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string? Notes { get; set; }
}

public class CreateHouseholdDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

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

// PSD2/Open Banking types
public class BankConnection
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

public enum ConnectionStatus
{
    Pending = 0,
    Active = 1,
    ConsentExpired = 2,
    Error = 3,
    Disconnected = 4
}

public class CreateBankConnectionDto
{
    public string BankId { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}

public class BankConnectionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? ConnectionId { get; set; }
}

public class ExternalAccount
{
    public int Id { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public ExternalAccountType AccountType { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
    public int? LinkedAccountId { get; set; }
    public string? LinkedAccountName { get; set; }
}

public enum ExternalAccountType
{
    Current = 0,
    Savings = 1,
    CreditCard = 2,
    Loan = 3,
    Other = 4
}

public class ExternalTransaction
{
    public int Id { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Counterpart { get; set; }
    public string? Reference { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsImported { get; set; }
}

public class ImportExternalTransactionsDto
{
    public int ExternalAccountId { get; set; }
    public int AccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class LinkAccountDto
{
    public int ExternalAccountId { get; set; }
    public int AccountId { get; set; }
}

public class Bank
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
}