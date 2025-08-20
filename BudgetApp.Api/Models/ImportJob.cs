using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Api.Models;

public class ImportJob
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = null!;

    public int AccountId { get; set; }
    public virtual Account Account { get; set; } = null!;

    public ImportSource Source { get; set; } = ImportSource.CsvFile;

    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public ImportStatus Status { get; set; } = ImportStatus.Pending;

    [MaxLength(2000)]
    public string? ColumnMapping { get; set; } // JSON

    public DuplicatePolicy DuplicatePolicy { get; set; } = DuplicatePolicy.Skip;

    public int ProcessedRows { get; set; } = 0;
    public int ImportedTransactions { get; set; } = 0;
    public int SkippedRows { get; set; } = 0;
    public int ErrorRows { get; set; } = 0;

    [MaxLength(2000)]
    public string? ErrorLog { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum ImportSource
{
    CsvFile = 0,
    OfxFile = 1,
    PSD2Api = 2
}

public enum ImportStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum DuplicatePolicy
{
    Skip = 0,
    Overwrite = 1,
    CreateNew = 2
}