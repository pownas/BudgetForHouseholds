using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.Models;

public class User : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Language { get; set; } = "sv"; // Swedish default

    [MaxLength(3)]
    public string Currency { get; set; } = "SEK"; // Swedish kronor default

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<HouseholdMember> HouseholdMemberships { get; set; } = new List<HouseholdMember>();
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}