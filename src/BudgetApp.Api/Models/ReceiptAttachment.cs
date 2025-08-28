using System;

namespace BudgetApp.Api.Models
{
    public class ReceiptAttachment
    {
        public int Id { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required string BlobPath { get; set; } // Path in object storage
        public DateTime UploadedAt { get; set; }
        public int TransactionId { get; set; }
    public required Transaction Transaction { get; set; }
    }
}
