using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Api.DTOs.Auth;
using WorkFlow.Api.DTOs.User;
using WorkFlow.Api.Services;

namespace WorkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly UserService _users;
    private readonly ICurrentUser _currentUser;

    public AuthController(AuthService auth, UserService users, ICurrentUser currentUser)
    {
        _auth = auth;
        _users = users;
        _currentUser = currentUser;
    }

    // POST: api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        if (result is null) return Unauthorized("Feil e-post eller passord.");
        return Ok(result);
    }

    // POST: api/auth/register-company
    [AllowAnonymous]
    [HttpPost("register-company")]
    public async Task<ActionResult<AuthResponseDto>> RegisterCompany(RegisterCompanyDto dto)
    {
        try
        {
            var result = await _auth.RegisterCompanyAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId);
        if (user is null) return Unauthorized();
        return Ok(user);
    }
}