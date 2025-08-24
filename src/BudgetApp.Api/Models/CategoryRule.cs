using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Api.Models;

public class CategoryRule
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public string? UserId { get; set; }
    public virtual User? User { get; set; }

    public int? HouseholdId { get; set; }
    public virtual Household? Household { get; set; }

    // Rule conditions
    [MaxLength(500)]
    public string? DescriptionContains { get; set; }

    [MaxLength(200)]
    public string? CounterpartContains { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxAmount { get; set; }

    // Rule actions
    public bool AutoCategorize { get; set; } = true;

    public bool AutoSplit { get; set; } = false;

    [MaxLength(500)]
    public string? AutoSplitConfig { get; set; } // JSON for split configuration

    [MaxLength(200)]
    public string? AutoTags { get; set; } // Comma-separated tags

    public int Priority { get; set; } = 0; // Higher number = higher priority

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Settlement
{
    public int Id { get; set; }

    public string FromUserId { get; set; } = string.Empty;
    public virtual User FromUser { get; set; } = null!;

    public string ToUserId { get; set; } = string.Empty;
    public virtual User ToUser { get; set; } = null!;

    public int HouseholdId { get; set; }
    public virtual Household Household { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? SettledDate { get; set; }

    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

    [MaxLength(200)]
    public string? Reference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public enum SettlementStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2
}