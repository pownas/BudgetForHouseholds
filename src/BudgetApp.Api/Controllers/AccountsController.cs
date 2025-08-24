using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly BudgetAppDbContext _context;

    public AccountsController(BudgetAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Account>>> GetAccounts()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var accounts = await _context.Accounts
            .Where(a => a.OwnerId == userId)
            .Include(a => a.Household)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var account = await _context.Accounts
            .Include(a => a.Household)
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);

        if (account == null)
        {
            return NotFound();
        }

        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount(CreateAccountDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        // Verify household access if specified
        if (dto.HouseholdId.HasValue)
        {
            var isMember = await _context.HouseholdMembers
                .AnyAsync(hm => hm.HouseholdId == dto.HouseholdId.Value && hm.UserId == userId);

            if (!isMember)
            {
                return Forbid("You are not a member of this household");
            }
        }

        var account = new Account
        {
            Name = dto.Name,
            Type = dto.Type,
            OwnerId = userId,
            HouseholdId = dto.HouseholdId,
            SharingType = dto.SharingType,
            CurrentBalance = dto.CurrentBalance,
            Description = dto.Description
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Account>> UpdateAccount(int id, CreateAccountDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);

        if (account == null)
        {
            return NotFound();
        }

        // Verify household access if changing household
        if (dto.HouseholdId.HasValue && dto.HouseholdId != account.HouseholdId)
        {
            var isMember = await _context.HouseholdMembers
                .AnyAsync(hm => hm.HouseholdId == dto.HouseholdId.Value && hm.UserId == userId);

            if (!isMember)
            {
                return Forbid("You are not a member of this household");
            }
        }

        account.Name = dto.Name;
        account.Type = dto.Type;
        account.HouseholdId = dto.HouseholdId;
        account.SharingType = dto.SharingType;
        account.CurrentBalance = dto.CurrentBalance;
        account.Description = dto.Description;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(account);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);

        if (account == null)
        {
            return NotFound();
        }

        // Check if account has transactions
        if (account.Transactions.Any())
        {
            return BadRequest("Cannot delete account with existing transactions");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/balance")]
    public async Task<ActionResult<object>> GetAccountBalance(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);

        if (account == null)
        {
            return NotFound();
        }

        // Calculate actual balance from transactions
        var transactionSum = await _context.Transactions
            .Where(t => t.AccountId == id)
            .SumAsync(t => t.Amount);

        var currentBalance = account.CurrentBalance + transactionSum;

        return Ok(new
        {
            accountId = account.Id,
            currentBalance = currentBalance,
            lastUpdated = DateTime.UtcNow
        });
    }
}