using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }
    public virtual Category? Parent { get; set; }

    public CategoryScope Scope { get; set; } = CategoryScope.User;

    public string? UserId { get; set; }
    public virtual User? User { get; set; }

    public int? HouseholdId { get; set; }
    public virtual Household? Household { get; set; }

    [MaxLength(7)]
    public string Color { get; set; } = "#000000"; // Hex color

    [MaxLength(50)]
    public string? Icon { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public virtual ICollection<CategoryRule> Rules { get; set; } = new List<CategoryRule>();
}

public enum CategoryScope
{
    User = 0,
    Household = 1,
    System = 2 // Predefined categories
}