using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public class HouseholdService : IHouseholdService
{
    private readonly BudgetAppDbContext _context;
    private readonly UserManager<User> _userManager;

    public HouseholdService(BudgetAppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Household> CreateHouseholdAsync(CreateHouseholdDto dto, string userId)
    {
        var household = new Household
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Households.Add(household);
        await _context.SaveChangesAsync();

        // Add creator as administrator
        var householdMember = new HouseholdMember
        {
            HouseholdId = household.Id,
            UserId = userId,
            Role = HouseholdRole.Administrator,
            JoinedAt = DateTime.UtcNow
        };

        _context.HouseholdMembers.Add(householdMember);
        await _context.SaveChangesAsync();

        return await GetHouseholdAsync(household.Id, userId);
    }

    public async Task<List<Household>> GetUserHouseholdsAsync(string userId)
    {
        return await _context.HouseholdMembers
            .Where(hm => hm.UserId == userId)
            .Include(hm => hm.Household)
                .ThenInclude(h => h.Members)
                    .ThenInclude(m => m.User)
            .Select(hm => hm.Household)
            .ToListAsync();
    }

    public async Task<Household> GetHouseholdAsync(int id, string userId)
    {
        var household = await _context.Households
            .Include(h => h.Members)
                .ThenInclude(m => m.User)
            .Include(h => h.SharedAccounts)
            .Include(h => h.Categories)
            .Include(h => h.Budgets)
            .FirstOrDefaultAsync(h => h.Id == id && h.Members.Any(m => m.UserId == userId));

        if (household == null)
        {
            throw new UnauthorizedAccessException("Household not found or access denied");
        }

        return household;
    }

    public async Task<bool> AddMemberAsync(int householdId, string email, string userId)
    {
        // Check if user is administrator
        var isAdmin = await _context.HouseholdMembers
            .AnyAsync(hm => hm.HouseholdId == householdId && 
                           hm.UserId == userId && 
                           hm.Role == HouseholdRole.Administrator);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only administrators can add members");
        }

        // Find user by email
        var userToAdd = await _userManager.FindByEmailAsync(email);
        if (userToAdd == null)
        {
            return false; // User not found
        }

        // Check if already a member
        var existingMember = await _context.HouseholdMembers
            .FirstOrDefaultAsync(hm => hm.HouseholdId == householdId && hm.UserId == userToAdd.Id);

        if (existingMember != null)
        {
            return false; // Already a member
        }

        var householdMember = new HouseholdMember
        {
            HouseholdId = householdId,
            UserId = userToAdd.Id,
            Role = HouseholdRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.HouseholdMembers.Add(householdMember);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int householdId, string memberUserId, string userId)
    {
        // Check if user is administrator
        var isAdmin = await _context.HouseholdMembers
            .AnyAsync(hm => hm.HouseholdId == householdId && 
                           hm.UserId == userId && 
                           hm.Role == HouseholdRole.Administrator);

        if (!isAdmin && userId != memberUserId)
        {
            throw new UnauthorizedAccessException("Only administrators can remove members, or users can remove themselves");
        }

        var member = await _context.HouseholdMembers
            .FirstOrDefaultAsync(hm => hm.HouseholdId == householdId && hm.UserId == memberUserId);

        if (member == null)
        {
            return false;
        }

        // Don't allow removing the last administrator
        if (member.Role == HouseholdRole.Administrator)
        {
            var adminCount = await _context.HouseholdMembers
                .CountAsync(hm => hm.HouseholdId == householdId && hm.Role == HouseholdRole.Administrator);

            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last administrator");
            }
        }

        _context.HouseholdMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Settlement>> CalculateSettlementsAsync(int householdId)
    {
        // Get all shared transactions in the household
        var sharedTransactions = await _context.Transactions
            .Include(t => t.Splits)
            .Where(t => t.Account.HouseholdId == householdId && 
                       t.SharingStatus == SharingStatus.SharedInHousehold)
            .ToListAsync();

        // Get household members
        var members = await _context.HouseholdMembers
            .Where(hm => hm.HouseholdId == householdId)
            .ToListAsync();

        // Calculate balances
        var balances = new Dictionary<string, decimal>();
        
        foreach (var member in members)
        {
            balances[member.UserId] = 0;
        }

        foreach (var transaction in sharedTransactions)
        {
            if (transaction.Splits.Any())
            {
                // Use defined splits
                foreach (var split in transaction.Splits)
                {
                    if (balances.ContainsKey(split.UserId))
                    {
                        balances[split.UserId] += split.Amount;
                    }
                }
            }
            else
            {
                // Default equal split among all members
                var splitAmount = transaction.Amount / members.Count;
                foreach (var member in members)
                {
                    balances[member.UserId] += splitAmount;
                }
            }

            // Subtract from the account owner (who paid)
            var accountOwner = await _context.Accounts
                .Where(a => a.Id == transaction.AccountId)
                .Select(a => a.OwnerId)
                .FirstAsync();

            if (balances.ContainsKey(accountOwner))
            {
                balances[accountOwner] -= transaction.Amount;
            }
        }

        // Create settlements to balance out the amounts
        var settlements = new List<Settlement>();
        var debtors = balances.Where(b => b.Value > 0).OrderByDescending(b => b.Value).ToList();
        var creditors = balances.Where(b => b.Value < 0).OrderBy(b => b.Value).ToList();

        foreach (var debtor in debtors)
        {
            var debtAmount = debtor.Value;
            
            foreach (var creditor in creditors)
            {
                if (debtAmount <= 0) break;
                
                var creditAmount = Math.Abs(creditor.Value);
                if (creditAmount <= 0) continue;

                var settlementAmount = Math.Min(debtAmount, creditAmount);
                
                if (settlementAmount > 0.01m) // Only create settlements for amounts > 1 öre
                {
                    settlements.Add(new Settlement
                    {
                        FromUserId = debtor.Key,
                        ToUserId = creditor.Key,
                        HouseholdId = householdId,
                        Amount = settlementAmount,
                        CalculatedDate = DateTime.UtcNow,
                        Status = SettlementStatus.Pending
                    });

                    debtAmount -= settlementAmount;
                    balances[creditor.Key] += settlementAmount;
                }
            }
        }

        return settlements;
    }
}