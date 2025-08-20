using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.Models;

public class Household
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<HouseholdMember> Members { get; set; } = new List<HouseholdMember>();
    public virtual ICollection<Account> SharedAccounts { get; set; } = new List<Account>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}

public class HouseholdMember
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }
    public virtual Household Household { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    public HouseholdRole Role { get; set; } = HouseholdRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public enum HouseholdRole
{
    Member = 0,
    Administrator = 1
}