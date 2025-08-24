using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Api.Models;

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public virtual Account Account { get; set; } = null!;

    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = null!;

    public DateTime Date { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "SEK";

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Counterpart { get; set; }

    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public SharingStatus SharingStatus { get; set; } = SharingStatus.Private;

    [MaxLength(100)]
    public string? ExternalId { get; set; } // For import deduplication

    [MaxLength(100)]
    public string? ExternalReference { get; set; } // For PSD2 transaction reference

    public int? ReceiptAttachmentId { get; set; }
    public virtual ReceiptAttachment ReceiptAttachment { get; set; } = null!;
    public string? ImportHash { get; set; } // For duplicate detection

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<TransactionSplit> Splits { get; set; } = new List<TransactionSplit>();
    public virtual ICollection<TransactionTag> Tags { get; set; } = new List<TransactionTag>();
    public virtual ICollection<TransactionAttachment> Attachments { get; set; } = new List<TransactionAttachment>();
}

public class TransactionSplit
{
    public int Id { get; set; }

    public int TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    [Column(TypeName = "decimal(5,4)")]
    public decimal Percentage { get; set; } // 0.5 = 50%

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }
}

public class TransactionTag
{
    public int Id { get; set; }

    public int TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Tag { get; set; } = string.Empty;
}

public class TransactionAttachment
{
    public int Id { get; set; }

    public int TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [MaxLength(64)]
    public string? FileHash { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public enum SharingStatus
{
    Private = 0,
    SharedInHousehold = 1
}