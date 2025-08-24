using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BudgetApp.Api.Services;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class Psd2Controller : ControllerBase
{
    private readonly IPsd2Service _psd2Service;
    private readonly IPsd2EventLogService _eventLogService;
    private readonly ILogger<Psd2Controller> _logger;

    public Psd2Controller(IPsd2Service psd2Service, IPsd2EventLogService eventLogService, ILogger<Psd2Controller> logger)
    {
        _psd2Service = psd2Service;
        _eventLogService = eventLogService;
        _logger = logger;
    }

    [HttpGet("connections")]
    public async Task<ActionResult<List<BankConnectionDto>>> GetBankConnections()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var connections = await _psd2Service.GetUserBankConnectionsAsync(userId);
        return Ok(connections);
    }

    [HttpPost("connections")]
    public async Task<ActionResult<BankConnectionResultDto>> CreateBankConnection(CreateBankConnectionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _psd2Service.CreateBankConnectionAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("connections/{connectionId}/complete")]
    public async Task<ActionResult> CompleteBankConnection(string connectionId, [FromBody] string authorizationCode)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var success = await _psd2Service.CompleteBankConnectionAsync(connectionId, authorizationCode, userId);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to complete bank connection" });
        }

        return Ok(new { message = "Bank connection completed successfully" });
    }

    [HttpDelete("connections/{connectionId}")]
    public async Task<ActionResult> DisconnectBank(int connectionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var success = await _psd2Service.DisconnectBankAsync(connectionId, userId);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to disconnect bank" });
        }

        return Ok(new { message = "Bank disconnected successfully" });
    }

    [HttpGet("connections/{connectionId}/accounts")]
    public async Task<ActionResult<List<ExternalAccountDto>>> GetExternalAccounts(int connectionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var accounts = await _psd2Service.GetExternalAccountsAsync(connectionId, userId);
        return Ok(accounts);
    }

    [HttpGet("accounts/{externalAccountId}/transactions")]
    public async Task<ActionResult<List<ExternalTransactionDto>>> GetExternalTransactions(
        int externalAccountId, 
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var transactions = await _psd2Service.GetExternalTransactionsAsync(externalAccountId, fromDate, toDate, userId);
        return Ok(transactions);
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportResult>> ImportExternalTransactions(ImportExternalTransactionsDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _psd2Service.ImportExternalTransactionsAsync(dto, userId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result);
    }

    [HttpPost("connections/{connectionId}/sync")]
    public async Task<ActionResult> SyncBankConnection(int connectionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var success = await _psd2Service.SyncBankConnectionAsync(connectionId, userId);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to sync bank connection" });
        }

        return Ok(new { message = "Bank connection synced successfully" });
    }

    [HttpPost("accounts/link")]
    public async Task<ActionResult> LinkAccount(LinkAccountDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var success = await _psd2Service.LinkAccountAsync(dto, userId);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to link accounts" });
        }

        return Ok(new { message = "Accounts linked successfully" });
    }

    [HttpGet("consent-check")]
    public async Task<ActionResult> CheckConsentExpiry()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var hasExpiring = await _psd2Service.CheckConsentExpiryAsync(userId);
        
        return Ok(new { hasExpiringConsents = hasExpiring });
    }

    [HttpGet("banks")]
    public ActionResult GetAvailableBanks()
    {
        // In a real implementation, this would fetch from the aggregator
        var banks = new[]
        {
            new { Id = "SWEDSESS", Name = "Swedbank", LogoUrl = "/images/swedbank.png" },
            new { Id = "HANDSESS", Name = "Handelsbanken", LogoUrl = "/images/handelsbanken.png" },
            new { Id = "SBABSESS", Name = "SEB", LogoUrl = "/images/seb.png" },
            new { Id = "NORDSESS", Name = "Nordea", LogoUrl = "/images/nordea.png" },
            new { Id = "ICANORDIC", Name = "ICA Banken", LogoUrl = "/images/ica.png" },
            new { Id = "FORXSESS", Name = "Forex Bank", LogoUrl = "/images/forex.png" }
        };

        return Ok(banks);
    }

    [HttpGet("event-logs")]
    public async Task<ActionResult<List<Psd2EventLogDto>>> GetEventLogs(
        [FromQuery] int? bankConnectionId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var eventLogs = await _eventLogService.GetEventLogsAsync(userId, bankConnectionId, fromDate, toDate, page, pageSize);
        return Ok(eventLogs);
    }
}