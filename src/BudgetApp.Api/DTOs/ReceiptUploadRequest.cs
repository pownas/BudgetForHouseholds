using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.DTOs;

public class ReceiptUploadRequest
{
    [Required]
    public int TransactionId { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;
}
