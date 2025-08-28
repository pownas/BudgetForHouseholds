using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetApp.Api.Models;
using BudgetApp.Api.Services;
using System.Threading.Tasks;
using System.IO;

namespace BudgetApp.Api.Controllers
{
    [ApiController]
    [Route("api/receipt-attachments")]
    public class ReceiptAttachmentController : ControllerBase
    {
        private readonly IReceiptAttachmentService _service;

        public ReceiptAttachmentController(IReceiptAttachmentService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadReceipt([FromForm] DTOs.ReceiptUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Ingen fil vald.");

            var result = await _service.UploadReceiptAsync(dto.TransactionId, dto.File);
            if (result == null)
                return BadRequest("Kunde inte ladda upp kvitto.");

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReceipt(int id)
        {
            var receipt = await _service.GetReceiptAsync(id);
            if (receipt == null)
                return NotFound();

            var stream = await _service.GetReceiptStreamAsync(receipt);
            return File(stream, receipt.ContentType, receipt.FileName);
        }
    }
}
