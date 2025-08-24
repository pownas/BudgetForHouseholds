using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Api.Models;

public class Budget
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;

    public BudgetScope Scope { get; set; } = BudgetScope.Individual;

    public string? UserId { get; set; }
    public virtual User? User { get; set; }

    public int? HouseholdId { get; set; }
    public virtual Household? Household { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal NotificationThreshold75 { get; set; } = 75.0m; // 75%

    [Column(TypeName = "decimal(5,2)")]
    public decimal NotificationThreshold100 { get; set; } = 100.0m; // 100%

    [Column(TypeName = "decimal(5,2)")]
    public decimal NotificationThreshold120 { get; set; } = 120.0m; // 120%

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum BudgetPeriod
{
    Weekly = 0,
    Monthly = 1,
    Quarterly = 2,
    Yearly = 3
}

public enum BudgetScope
{
    Individual = 0,
    Household = 1
}