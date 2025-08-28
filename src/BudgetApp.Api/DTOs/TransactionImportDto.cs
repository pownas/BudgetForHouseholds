using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BudgetApp.Api.DTOs
{
    public class TransactionImportDto
    {
        [FromForm]
        public required IFormFile File { get; set; }
    }
}
