// ============================================================
//  CMT.API – UsersController  (Admin only)
//  GET    /api/users
//  GET    /api/users/{id}
//  POST   /api/users
//  PUT    /api/users/{id}
// ============================================================
using CMT.API.Models;
using CMT.Core.DTOs.Auth;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;
    public UsersController(IAuthService auth) => _auth = auth;

    /// <summary>Get all system users. Admin only.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _auth.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(users));
    }

    /// <summary>Get a single user by ID. Admin only.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _auth.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Create a new user (Admin or Staff). Admin only.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var user = await _auth.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<UserDto>.Ok(user, "User created successfully."));
    }

    /// <summary>Update an existing user. Admin only.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var user = await _auth.UpdateUserAsync(id, request);
        return Ok(ApiResponse<UserDto>.Ok(user, "User updated successfully."));
    }
}
