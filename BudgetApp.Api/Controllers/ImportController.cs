using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BudgetApp.Api.Services;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly ICsvImportService _csvImportService;

    public ImportController(ICsvImportService csvImportService)
    {
        _csvImportService = csvImportService;
    }

    [HttpPost("csv/preview")]
    public async Task<ActionResult<List<CsvPreviewRow>>> PreviewCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are supported");
        }

        try
        {
            using var stream = file.OpenReadStream();
            var preview = await _csvImportService.PreviewCsvAsync(stream, file.FileName);
            return Ok(preview);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing CSV file: {ex.Message}");
        }
    }

    [HttpPost("csv")]
    public async Task<ActionResult<ImportResult>> ImportCsv(IFormFile file, [FromForm] int accountId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are supported");
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
        {
            return BadRequest("File size exceeds 10MB limit");
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            
            using var stream = file.OpenReadStream();
            var result = await _csvImportService.ImportCsvAsync(stream, file.FileName, accountId, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("Access denied to the specified account");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error importing CSV file: {ex.Message}");
        }
    }
}