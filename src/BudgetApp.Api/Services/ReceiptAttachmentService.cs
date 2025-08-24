using Microsoft.AspNetCore.Http;
using BudgetApp.Api.Models;
using BudgetApp.Api.Data;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace BudgetApp.Api.Services
{
    public class ReceiptAttachmentService : IReceiptAttachmentService
    {
        private readonly BudgetAppDbContext _db;
        private readonly string _storagePath = "./Receipts";

        public ReceiptAttachmentService(BudgetAppDbContext db)
        {
            _db = db;
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        public async Task<ReceiptAttachment?> UploadReceiptAsync(int transactionId, IFormFile file)
        {
            var transaction = _db.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transaction == null) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_storagePath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new ReceiptAttachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                BlobPath = filePath,
                UploadedAt = DateTime.UtcNow,
                TransactionId = transactionId
            };
            _db.ReceiptAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            transaction.ReceiptAttachmentId = attachment.Id;
            transaction.ReceiptAttachment = attachment;
            await _db.SaveChangesAsync();

            return attachment;
        }

        public async Task<ReceiptAttachment?> GetReceiptAsync(int id)
        {
            return await Task.FromResult(_db.ReceiptAttachments.FirstOrDefault(a => a.Id == id));
        }

        public async Task<Stream> GetReceiptStreamAsync(ReceiptAttachment attachment)
        {
            var stream = new FileStream(attachment.BlobPath, FileMode.Open, FileAccess.Read);
            return await Task.FromResult(stream);
        }
    }
}
