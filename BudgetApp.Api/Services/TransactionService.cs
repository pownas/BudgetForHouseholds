using Microsoft.EntityFrameworkCore;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly BudgetAppDbContext _context;

    public TransactionService(BudgetAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string userId, int? accountId = null, int? householdId = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Splits)
            .Include(t => t.Tags)
            .Where(t => t.Account.OwnerId == userId);

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId.Value);
        }

        if (householdId.HasValue)
        {
            // Include transactions from household shared accounts
            query = query.Where(t => t.Account.HouseholdId == householdId.Value && 
                                    t.SharingStatus == SharingStatus.SharedInHousehold);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction> CreateTransactionAsync(CreateTransactionDto dto, string userId)
    {
        // Verify user owns the account
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.OwnerId == userId);

        if (account == null)
        {
            throw new UnauthorizedAccessException("Account not found or access denied");
        }

        var transaction = new Transaction
        {
            AccountId = dto.AccountId,
            Date = dto.Date,
            Amount = dto.Amount,
            Description = dto.Description,
            Counterpart = dto.Counterpart,
            CategoryId = dto.CategoryId,
            Notes = dto.Notes,
            SharingStatus = dto.SharingStatus
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Add splits if provided
        if (dto.Splits?.Any() == true)
        {
            foreach (var splitDto in dto.Splits)
            {
                var split = new TransactionSplit
                {
                    TransactionId = transaction.Id,
                    UserId = splitDto.UserId,
                    Percentage = splitDto.Percentage,
                    Amount = transaction.Amount * splitDto.Percentage,
                    Notes = splitDto.Notes
                };
                _context.TransactionSplits.Add(split);
            }
        }

        // Add tags if provided
        if (dto.Tags?.Any() == true)
        {
            foreach (var tag in dto.Tags)
            {
                var transactionTag = new TransactionTag
                {
                    TransactionId = transaction.Id,
                    Tag = tag
                };
                _context.TransactionTags.Add(transactionTag);
            }
        }

        await _context.SaveChangesAsync();

        // Apply category rules if no category was specified
        if (transaction.CategoryId == null)
        {
            await ApplyCategoryRulesAsync(transaction.Id);
        }

        return await GetTransactionWithDetailsAsync(transaction.Id);
    }

    public async Task<Transaction> UpdateTransactionAsync(int id, UpdateTransactionDto dto, string userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Splits)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.OwnerId == userId);

        if (transaction == null)
        {
            throw new UnauthorizedAccessException("Transaction not found or access denied");
        }

        // Update properties
        if (dto.Date.HasValue) transaction.Date = dto.Date.Value;
        if (dto.Amount.HasValue) transaction.Amount = dto.Amount.Value;
        if (dto.Description != null) transaction.Description = dto.Description;
        if (dto.Counterpart != null) transaction.Counterpart = dto.Counterpart;
        if (dto.CategoryId.HasValue) transaction.CategoryId = dto.CategoryId.Value;
        if (dto.Notes != null) transaction.Notes = dto.Notes;
        if (dto.SharingStatus.HasValue) transaction.SharingStatus = dto.SharingStatus.Value;

        transaction.UpdatedAt = DateTime.UtcNow;

        // Update splits if provided
        if (dto.Splits != null)
        {
            // Remove existing splits
            _context.TransactionSplits.RemoveRange(transaction.Splits);
            
            // Add new splits
            foreach (var splitDto in dto.Splits)
            {
                var split = new TransactionSplit
                {
                    TransactionId = transaction.Id,
                    UserId = splitDto.UserId,
                    Percentage = splitDto.Percentage,
                    Amount = transaction.Amount * splitDto.Percentage,
                    Notes = splitDto.Notes
                };
                _context.TransactionSplits.Add(split);
            }
        }

        // Update tags if provided
        if (dto.Tags != null)
        {
            // Remove existing tags
            _context.TransactionTags.RemoveRange(transaction.Tags);
            
            // Add new tags
            foreach (var tag in dto.Tags)
            {
                var transactionTag = new TransactionTag
                {
                    TransactionId = transaction.Id,
                    Tag = tag
                };
                _context.TransactionTags.Add(transactionTag);
            }
        }

        await _context.SaveChangesAsync();
        return await GetTransactionWithDetailsAsync(transaction.Id);
    }

    public async Task<bool> DeleteTransactionAsync(int id, string userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.OwnerId == userId);

        if (transaction == null)
        {
            return false;
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ApplyCategoryRulesAsync(int transactionId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId);

        if (transaction == null) return;

        var rules = await _context.CategoryRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (RuleMatches(rule, transaction))
            {
                transaction.CategoryId = rule.CategoryId;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                break;
            }
        }
    }

    private async Task<Transaction> GetTransactionWithDetailsAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Splits)
            .Include(t => t.Tags)
            .FirstAsync(t => t.Id == id);
    }

    private bool RuleMatches(CategoryRule rule, Transaction transaction)
    {
        if (!string.IsNullOrEmpty(rule.DescriptionContains) && 
            !transaction.Description.Contains(rule.DescriptionContains, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(rule.CounterpartContains) && 
            (string.IsNullOrEmpty(transaction.Counterpart) || 
             !transaction.Counterpart.Contains(rule.CounterpartContains, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (rule.MinAmount.HasValue && transaction.Amount < rule.MinAmount.Value)
        {
            return false;
        }

        if (rule.MaxAmount.HasValue && transaction.Amount > rule.MaxAmount.Value)
        {
            return false;
        }

        return true;
    }
}