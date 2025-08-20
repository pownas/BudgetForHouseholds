using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BudgetApp.Api.Services;
using BudgetApp.Api.DTOs;
using BudgetApp.Api.Models;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HouseholdsController : ControllerBase
{
    private readonly IHouseholdService _householdService;

    public HouseholdsController(IHouseholdService householdService)
    {
        _householdService = householdService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Household>>> GetHouseholds()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var households = await _householdService.GetUserHouseholdsAsync(userId);
        return Ok(households);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Household>> GetHousehold(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var household = await _householdService.GetHouseholdAsync(id, userId);
            return Ok(household);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<ActionResult<Household>> CreateHousehold(CreateHouseholdDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var household = await _householdService.CreateHouseholdAsync(dto, userId);
        return CreatedAtAction(nameof(GetHousehold), new { id = household.Id }, household);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] string email)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var success = await _householdService.AddMemberAsync(id, email, userId);
            
            if (!success)
            {
                return BadRequest("User not found or already a member");
            }

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(int id, string memberUserId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var success = await _householdService.RemoveMemberAsync(id, memberUserId, userId);
            
            if (!success)
            {
                return NotFound("Member not found");
            }

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/settlements")]
    public async Task<ActionResult<List<Settlement>>> GetSettlements(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            
            // Verify user is member of household
            await _householdService.GetHouseholdAsync(id, userId);
            
            var settlements = await _householdService.CalculateSettlementsAsync(id);
            return Ok(settlements);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}