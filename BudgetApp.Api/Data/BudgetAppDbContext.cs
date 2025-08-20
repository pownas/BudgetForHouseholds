using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Api.Models;

namespace BudgetApp.Api.Data;

public class BudgetAppDbContext : IdentityDbContext<User>
{
    public BudgetAppDbContext(DbContextOptions<BudgetAppDbContext> options) : base(options)
    {
    }

    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionSplit> TransactionSplits { get; set; }
    public DbSet<TransactionTag> TransactionTags { get; set; }
    public DbSet<TransactionAttachment> TransactionAttachments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryRule> CategoryRules { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<HouseholdMember>()
            .HasKey(hm => hm.Id);

        modelBuilder.Entity<HouseholdMember>()
            .HasOne(hm => hm.Household)
            .WithMany(h => h.Members)
            .HasForeignKey(hm => hm.HouseholdId);

        modelBuilder.Entity<HouseholdMember>()
            .HasOne(hm => hm.User)
            .WithMany(u => u.HouseholdMemberships)
            .HasForeignKey(hm => hm.UserId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Owner)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.OwnerId);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Household)
            .WithMany(h => h.SharedAccounts)
            .HasForeignKey(a => a.HouseholdId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TransactionSplit>()
            .HasOne(ts => ts.Transaction)
            .WithMany(t => t.Splits)
            .HasForeignKey(ts => ts.TransactionId);

        modelBuilder.Entity<TransactionSplit>()
            .HasOne(ts => ts.User)
            .WithMany()
            .HasForeignKey(ts => ts.UserId);

        modelBuilder.Entity<TransactionTag>()
            .HasOne(tt => tt.Transaction)
            .WithMany(t => t.Tags)
            .HasForeignKey(tt => tt.TransactionId);

        modelBuilder.Entity<TransactionAttachment>()
            .HasOne(ta => ta.Transaction)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TransactionId);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Household)
            .WithMany(h => h.Categories)
            .HasForeignKey(c => c.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryRule>()
            .HasOne(cr => cr.Category)
            .WithMany(c => c.Rules)
            .HasForeignKey(cr => cr.CategoryId);

        modelBuilder.Entity<CategoryRule>()
            .HasOne(cr => cr.User)
            .WithMany()
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CategoryRule>()
            .HasOne(cr => cr.Household)
            .WithMany()
            .HasForeignKey(cr => cr.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Household)
            .WithMany(h => h.Budgets)
            .HasForeignKey(b => b.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Settlement>()
            .HasOne(s => s.FromUser)
            .WithMany()
            .HasForeignKey(s => s.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Settlement>()
            .HasOne(s => s.ToUser)
            .WithMany()
            .HasForeignKey(s => s.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Settlement>()
            .HasOne(s => s.Household)
            .WithMany()
            .HasForeignKey(s => s.HouseholdId);

        modelBuilder.Entity<ImportJob>()
            .HasOne(ij => ij.User)
            .WithMany()
            .HasForeignKey(ij => ij.UserId);

        modelBuilder.Entity<ImportJob>()
            .HasOne(ij => ij.Account)
            .WithMany()
            .HasForeignKey(ij => ij.AccountId);

        // Configure indexes
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.ImportHash)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.ExternalId);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.Date);

        modelBuilder.Entity<HouseholdMember>()
            .HasIndex(hm => new { hm.HouseholdId, hm.UserId })
            .IsUnique();

        // Seed default categories
        SeedDefaultCategories(modelBuilder);
    }

    private void SeedDefaultCategories(ModelBuilder modelBuilder)
    {
        var defaultCategories = new[]
        {
            new Category { Id = 1, Name = "Boende", Color = "#FF6B6B", Icon = "home", Scope = CategoryScope.System },
            new Category { Id = 2, Name = "Mat", Color = "#4ECDC4", Icon = "restaurant", Scope = CategoryScope.System },
            new Category { Id = 3, Name = "Transport", Color = "#45B7D1", Icon = "directions_car", Scope = CategoryScope.System },
            new Category { Id = 4, Name = "Försäkring", Color = "#96CEB4", Icon = "security", Scope = CategoryScope.System },
            new Category { Id = 5, Name = "Barn", Color = "#FFEAA7", Icon = "child_care", Scope = CategoryScope.System },
            new Category { Id = 6, Name = "Nöje", Color = "#DDA0DD", Icon = "movie", Scope = CategoryScope.System },
            new Category { Id = 7, Name = "Hälsa", Color = "#98D8C8", Icon = "local_hospital", Scope = CategoryScope.System },
            new Category { Id = 8, Name = "Inkomst", Color = "#81C784", Icon = "account_balance_wallet", Scope = CategoryScope.System },
            new Category { Id = 9, Name = "Övrigt", Color = "#B0BEC5", Icon = "category", Scope = CategoryScope.System }
        };

        modelBuilder.Entity<Category>().HasData(defaultCategories);
    }
}