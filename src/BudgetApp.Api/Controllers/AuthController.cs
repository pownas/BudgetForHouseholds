using Microsoft.AspNetCore.Mvc;
using BudgetApp.Api.Services;
using BudgetApp.Api.DTOs;

namespace BudgetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                firstName = result.User.FirstName,
                lastName = result.User.LastName
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginDto model)
    {
        var result = await _authService.LoginAsync(model);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                firstName = result.User.FirstName,
                lastName = result.User.LastName
            }
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] string token)
    {
        var result = await _authService.RefreshTokenAsync(token);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                firstName = result.User.FirstName,
                lastName = result.User.LastName
            }
        });
    }
}