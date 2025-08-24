using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.Models;

/// <summary>
/// Represents an event log entry for PSD2/Open Banking activities
/// Used for monitoring connection status and synchronization events
/// </summary>
public class Psd2EventLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    public int? BankConnectionId { get; set; }
    public virtual BankConnection? BankConnection { get; set; }

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty; // e.g., "CONNECTION_CREATED", "SYNC_COMPLETED", "CONSENT_EXPIRED"

    [Required]
    [MaxLength(200)]
    public string EventDescription { get; set; } = string.Empty;

    public string? EventData { get; set; } // JSON data for additional context

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? UserAgent { get; set; }

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Standard event types for PSD2 operations
/// </summary>
public static class Psd2EventTypes
{
    public const string ConnectionCreated = "CONNECTION_CREATED";
    public const string ConnectionCompleted = "CONNECTION_COMPLETED";
    public const string ConnectionFailed = "CONNECTION_FAILED";
    public const string ConnectionDisconnected = "CONNECTION_DISCONNECTED";
    public const string SyncStarted = "SYNC_STARTED";
    public const string SyncCompleted = "SYNC_COMPLETED";
    public const string SyncFailed = "SYNC_FAILED";
    public const string ConsentExpired = "CONSENT_EXPIRED";
    public const string ConsentRenewed = "CONSENT_RENEWED";
    public const string TransactionsImported = "TRANSACTIONS_IMPORTED";
    public const string AccountLinked = "ACCOUNT_LINKED";
    public const string AccountUnlinked = "ACCOUNT_UNLINKED";
    public const string ErrorOccurred = "ERROR_OCCURRED";
}