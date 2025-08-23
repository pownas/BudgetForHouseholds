using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Api.Models;

// Represents a connection to a bank via PSD2/Open Banking
public class BankConnection
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string BankId { get; set; } = string.Empty; // e.g., SWEDSESS for Swedbank

    [Required]
    [MaxLength(200)]
    public string ExternalConnectionId { get; set; } = string.Empty; // ID from aggregator

    public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

    [Required]
    public DateTime ConsentGivenAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConsentExpiresAt { get; set; }

    public DateTime? LastSyncAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ExternalAccount> ExternalAccounts { get; set; } = new List<ExternalAccount>();
}

// Represents an external bank account accessible via PSD2
public class ExternalAccount
{
    public int Id { get; set; }

    [Required]
    public int BankConnectionId { get; set; }
    public virtual BankConnection BankConnection { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ExternalAccountId { get; set; } = string.Empty; // ID from bank/aggregator

    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty; // Masked or partial number

    public ExternalAccountType AccountType { get; set; } = ExternalAccountType.Current;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AvailableBalance { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "SEK";

    public bool IsActive { get; set; } = true;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Link to internal account if user has connected this external account
    public int? LinkedAccountId { get; set; }
    public virtual Account? LinkedAccount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Represents a transaction from external bank account
public class ExternalTransaction
{
    public int Id { get; set; }

    [Required]
    public int ExternalAccountId { get; set; }
    public virtual ExternalAccount ExternalAccount { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ExternalTransactionId { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Counterpart { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "SEK";

    public bool IsImported { get; set; } = false;

    // Link to internal transaction if imported
    public int? ImportedTransactionId { get; set; }
    public virtual Transaction? ImportedTransaction { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ConnectionStatus
{
    Pending = 0,
    Active = 1,
    ConsentExpired = 2,
    Error = 3,
    Disconnected = 4
}

public enum ExternalAccountType
{
    Current = 0,
    Savings = 1,
    CreditCard = 2,
    Loan = 3,
    Other = 4
}