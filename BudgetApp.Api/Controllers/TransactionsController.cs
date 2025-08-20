using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;
using BudgetApp.Api.Services;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly BudgetAppDbContext _context;

    public TransactionsController(ITransactionService transactionService, BudgetAppDbContext context)
    {
        _transactionService = transactionService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactions(
        [FromQuery] int? accountId = null, 
        [FromQuery] int? householdId = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var transactions = await _transactionService.GetTransactionsAsync(userId, accountId, householdId);
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Splits)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.OwnerId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(CreateTransactionDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var transaction = await _transactionService.CreateTransactionAsync(dto, userId);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Transaction>> UpdateTransaction(int id, UpdateTransactionDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var transaction = await _transactionService.UpdateTransactionAsync(id, dto, userId);
            return Ok(transaction);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var success = await _transactionService.DeleteTransactionAsync(id, userId);
        
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/apply-rules")]
    public async Task<IActionResult> ApplyRules(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        // Verify user owns this transaction
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.OwnerId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        await _transactionService.ApplyCategoryRulesAsync(id);
        return Ok();
    }
}