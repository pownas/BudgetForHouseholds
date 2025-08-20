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
            .Include(t => t.Attachments)
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

    [HttpPost("{id}/attachments")]
    public async Task<ActionResult<TransactionAttachment>> UploadAttachment(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType?.ToLower()))
        {
            return BadRequest("Only image files (JPEG, PNG, GIF) and PDF files are allowed");
        }

        // Validate file size (5MB limit)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size exceeds 5MB limit");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        // Verify user owns this transaction
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.Account.OwnerId == userId);

        if (transaction == null)
        {
            return NotFound("Transaction not found");
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "attachments");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Calculate file hash
            string fileHash;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    fileHash = Convert.ToBase64String(hashBytes);
                }
            }

            // Create attachment record
            var attachment = new TransactionAttachment
            {
                TransactionId = id,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileHash = fileHash,
                UploadedAt = DateTime.UtcNow
            };

            _context.TransactionAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttachment), new { transactionId = id, attachmentId = attachment.Id }, attachment);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error uploading file: {ex.Message}");
        }
    }

    [HttpGet("{transactionId}/attachments/{attachmentId}")]
    public async Task<ActionResult<TransactionAttachment>> GetAttachment(int transactionId, int attachmentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var attachment = await _context.TransactionAttachments
            .Include(a => a.Transaction)
            .ThenInclude(t => t.Account)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && 
                                    a.TransactionId == transactionId && 
                                    a.Transaction.Account.OwnerId == userId);

        if (attachment == null)
        {
            return NotFound();
        }

        return Ok(attachment);
    }

    [HttpGet("{transactionId}/attachments/{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int transactionId, int attachmentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var attachment = await _context.TransactionAttachments
            .Include(a => a.Transaction)
            .ThenInclude(t => t.Account)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && 
                                    a.TransactionId == transactionId && 
                                    a.Transaction.Account.OwnerId == userId);

        if (attachment == null)
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(attachment.FilePath))
        {
            return NotFound("File not found on disk");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
        return File(fileBytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
    }

    [HttpDelete("{transactionId}/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(int transactionId, int attachmentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        
        var attachment = await _context.TransactionAttachments
            .Include(a => a.Transaction)
            .ThenInclude(t => t.Account)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && 
                                    a.TransactionId == transactionId && 
                                    a.Transaction.Account.OwnerId == userId);

        if (attachment == null)
        {
            return NotFound();
        }

        try
        {
            // Delete file from disk
            if (System.IO.File.Exists(attachment.FilePath))
            {
                System.IO.File.Delete(attachment.FilePath);
            }

            // Delete attachment record
            _context.TransactionAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting attachment: {ex.Message}");
        }
    }
}