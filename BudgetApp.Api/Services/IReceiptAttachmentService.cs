using Microsoft.AspNetCore.Http;
using BudgetApp.Api.Models;
using System.IO;
using System.Threading.Tasks;

namespace BudgetApp.Api.Services
{
    public interface IReceiptAttachmentService
    {
        Task<ReceiptAttachment?> UploadReceiptAsync(int transactionId, IFormFile file);
        Task<ReceiptAttachment?> GetReceiptAsync(int id);
        Task<Stream> GetReceiptStreamAsync(ReceiptAttachment attachment);
    }
}
