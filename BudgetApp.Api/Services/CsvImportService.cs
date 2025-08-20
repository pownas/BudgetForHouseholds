using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Api.Data;
using BudgetApp.Api.Models;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Services;

public class CsvImportService : ICsvImportService
{
    private readonly BudgetAppDbContext _context;

    public CsvImportService(BudgetAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CsvPreviewRow>> PreviewCsvAsync(Stream csvStream, string fileName)
    {
        var preview = new List<CsvPreviewRow>();
        
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = DetectDelimiter(csvStream),
            HasHeaderRecord = true
        });

        // Reset stream
        csvStream.Position = 0;
        reader.DiscardBufferedData();

        // Read header
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToList() ?? new List<string>();

        // Read first 10 rows for preview
        var rowCount = 0;
        while (await csv.ReadAsync() && rowCount < 10)
        {
            var row = new CsvPreviewRow
            {
                RowNumber = rowCount + 1,
                Data = new Dictionary<string, string>()
            };

            foreach (var header in headers)
            {
                var value = csv.GetField(header) ?? "";
                row.Data[header] = value;
            }

            preview.Add(row);
            rowCount++;
        }

        return preview;
    }

    public async Task<ImportResult> ImportCsvAsync(Stream csvStream, string fileName, int accountId, string userId)
    {
        var result = new ImportResult { Success = true };
        var importedTransactions = new List<Transaction>();

        // Create import job
        var importJob = new ImportJob
        {
            UserId = userId,
            AccountId = accountId,
            Source = ImportSource.CsvFile,
            FileName = fileName,
            FilePath = $"imports/{userId}/{DateTime.UtcNow:yyyyMMdd}/{fileName}",
            Status = ImportStatus.Processing,
            StartedAt = DateTime.UtcNow
        };

        _context.ImportJobs.Add(importJob);
        await _context.SaveChangesAsync();

        try
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DetectDelimiter(csvStream),
                HasHeaderRecord = true
            });

            // Reset stream
            csvStream.Position = 0;
            reader.DiscardBufferedData();

            await csv.ReadAsync();
            csv.ReadHeader();

            var rowNumber = 0;
            while (await csv.ReadAsync())
            {
                rowNumber++;
                importJob.ProcessedRows++;

                try
                {
                    var transaction = ParseCsvRow(csv, accountId);
                    
                    // Check for duplicates
                    var hash = GenerateTransactionHash(transaction);
                    var existingTransaction = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.ImportHash == hash);

                    if (existingTransaction != null)
                    {
                        importJob.SkippedRows++;
                        continue;
                    }

                    transaction.ImportHash = hash;
                    importedTransactions.Add(transaction);
                }
                catch (Exception ex)
                {
                    importJob.ErrorRows++;
                    var errorMsg = $"Row {rowNumber}: {ex.Message}";
                    importJob.ErrorLog = string.IsNullOrEmpty(importJob.ErrorLog) 
                        ? errorMsg 
                        : $"{importJob.ErrorLog}\n{errorMsg}";
                }
            }

            // Save transactions
            if (importedTransactions.Any())
            {
                _context.Transactions.AddRange(importedTransactions);
                await _context.SaveChangesAsync();
                
                importJob.ImportedTransactions = importedTransactions.Count;
                
                // Apply category rules
                foreach (var transaction in importedTransactions)
                {
                    await ApplyCategoryRules(transaction);
                }
                
                await _context.SaveChangesAsync();
            }

            importJob.Status = ImportStatus.Completed;
            importJob.CompletedAt = DateTime.UtcNow;
            
            result.ImportedCount = importJob.ImportedTransactions;
            result.SkippedCount = importJob.SkippedRows;
            result.ErrorCount = importJob.ErrorRows;
            result.Message = $"Imported {result.ImportedCount} transactions, skipped {result.SkippedCount}, errors {result.ErrorCount}";
        }
        catch (Exception ex)
        {
            importJob.Status = ImportStatus.Failed;
            importJob.ErrorLog = ex.Message;
            result.Success = false;
            result.Error = ex.Message;
        }

        await _context.SaveChangesAsync();
        return result;
    }

    private string DetectDelimiter(Stream csvStream)
    {
        var position = csvStream.Position;
        using var reader = new StreamReader(csvStream, leaveOpen: true);
        var firstLine = reader.ReadLine() ?? "";
        csvStream.Position = position;

        var commaCount = firstLine.Count(c => c == ',');
        var semicolonCount = firstLine.Count(c => c == ';');
        
        return semicolonCount > commaCount ? ";" : ",";
    }

    private Transaction ParseCsvRow(CsvReader csv, int accountId)
    {
        // Try to parse common CSV formats
        var date = ParseDate(csv.GetField("Date") ?? csv.GetField("Datum") ?? csv.GetField("Transaction Date") ?? "");
        var amount = ParseAmount(csv.GetField("Amount") ?? csv.GetField("Belopp") ?? csv.GetField("Summa") ?? "0");
        var description = csv.GetField("Description") ?? csv.GetField("Beskrivning") ?? csv.GetField("Text") ?? "";
        var counterpart = csv.GetField("Counterpart") ?? csv.GetField("Motpart") ?? "";

        return new Transaction
        {
            AccountId = accountId,
            Date = date,
            Amount = amount,
            Description = description.Trim(),
            Counterpart = string.IsNullOrEmpty(counterpart) ? null : counterpart.Trim(),
            Currency = "SEK"
        };
    }

    private DateTime ParseDate(string dateStr)
    {
        var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "dd.MM.yyyy", "yyyy-MM-dd HH:mm:ss" };
        
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
        }

        if (DateTime.TryParse(dateStr, out var parsedDate))
        {
            return parsedDate;
        }

        throw new FormatException($"Unable to parse date: {dateStr}");
    }

    private decimal ParseAmount(string amountStr)
    {
        // Handle different decimal separators and thousands separators
        amountStr = amountStr.Replace(" ", "").Replace("\u00A0", ""); // Remove spaces
        
        // Swedish format: 1 234,56 or 1234,56
        if (amountStr.Contains(',') && !amountStr.Contains('.'))
        {
            amountStr = amountStr.Replace(",", ".");
        }
        // Handle 1,234.56 format
        else if (amountStr.Contains(',') && amountStr.Contains('.'))
        {
            var lastComma = amountStr.LastIndexOf(',');
            var lastDot = amountStr.LastIndexOf('.');
            if (lastDot > lastComma)
            {
                amountStr = amountStr.Replace(",", "");
            }
            else
            {
                amountStr = amountStr.Replace(".", "").Replace(",", ".");
            }
        }

        if (decimal.TryParse(amountStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }

        throw new FormatException($"Unable to parse amount: {amountStr}");
    }

    private string GenerateTransactionHash(Transaction transaction)
    {
        var hashInput = $"{transaction.AccountId}|{transaction.Date:yyyy-MM-dd}|{transaction.Amount}|{transaction.Description}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task ApplyCategoryRules(Transaction transaction)
    {
        var rules = await _context.CategoryRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (RuleMatches(rule, transaction))
            {
                transaction.CategoryId = rule.CategoryId;
                break; // Apply first matching rule
            }
        }
    }

    private bool RuleMatches(CategoryRule rule, Transaction transaction)
    {
        // Check description contains
        if (!string.IsNullOrEmpty(rule.DescriptionContains) && 
            !transaction.Description.Contains(rule.DescriptionContains, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check counterpart contains
        if (!string.IsNullOrEmpty(rule.CounterpartContains) && 
            (string.IsNullOrEmpty(transaction.Counterpart) || 
             !transaction.Counterpart.Contains(rule.CounterpartContains, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Check amount range
        if (rule.MinAmount.HasValue && transaction.Amount < rule.MinAmount.Value)
        {
            return false;
        }

        if (rule.MaxAmount.HasValue && transaction.Amount > rule.MaxAmount.Value)
        {
            return false;
        }

        return true;
    }
}