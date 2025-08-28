using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetApp.Api.DTOs
{
    public class ReceiptUploadDto
    {
        [FromForm]
    public required IFormFile File { get; set; }
        [FromForm]
        public int TransactionId { get; set; }
    }
}
