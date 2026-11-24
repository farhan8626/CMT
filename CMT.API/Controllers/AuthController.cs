// ============================================================
//  CMT.API – AuthController
//  POST /api/auth/login
//  POST /api/auth/change-password   [Authorize]
//  GET  /api/auth/me                [Authorize]
// ============================================================
using CMT.API.Extensions;
using CMT.API.Models;
using CMT.Core.DTOs.Auth;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Login with email + password. Returns JWT Bearer token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful."));
    }

    /// <summary>Get the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        var user   = await _auth.GetUserByIdAsync(userId);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Change the current user's own password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId();
        await _auth.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }
}
