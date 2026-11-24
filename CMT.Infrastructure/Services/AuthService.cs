// ============================================================
//  CMT.Infrastructure – Auth Service
// ============================================================
using BCrypt.Net;
using CMT.Core.DTOs.Auth;
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Helpers;

namespace CMT.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly JwtTokenHelper  _jwt;

    public AuthService(IUserRepository repo, JwtTokenHelper jwt)
    {
        _repo = repo;
        _jwt  = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repo.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated. Contact admin.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return new LoginResponse(user.Id, user.FullName, user.Email, user.Role, token, expiresAt);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        if (await _repo.EmailExistsAsync(request.Email))
            throw new ArgumentException($"Email '{request.Email}' is already in use.");

        var user = new User
        {
            FullName     = request.FullName.Trim(),
            Email        = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Role         = request.Role,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        await _repo.CreateAsync(user);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        if (await _repo.EmailExistsAsync(request.Email, excludeId: id))
            throw new ArgumentException($"Email '{request.Email}' is already in use.");

        user.FullName = request.FullName.Trim();
        user.Email    = request.Email.Trim().ToLower();
        user.Role     = request.Role;
        user.IsActive = request.IsActive;

        await _repo.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _repo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        await _repo.UpdateAsync(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync() =>
        await _repo.GetAllAsync();

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");
        return MapToDto(user);
    }

    private static UserDto MapToDto(User u) =>
        new(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt);
}
