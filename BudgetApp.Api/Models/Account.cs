using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Api.Models;

public class Account
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; } = AccountType.BankAccount;

    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = null!;

    public int? HouseholdId { get; set; }
    public virtual Household? Household { get; set; }

    public SharingType SharingType { get; set; } = SharingType.Private;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    [MaxLength(3)]
    public string Currency { get; set; } = "SEK";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
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