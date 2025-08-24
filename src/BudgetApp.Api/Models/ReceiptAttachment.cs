using System;

namespace BudgetApp.Api.Models
{
    public class ReceiptAttachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string BlobPath { get; set; } // Path in object storage
        public DateTime UploadedAt { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
    }
}
