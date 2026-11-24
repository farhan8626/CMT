// ============================================================
//  CMT.Core – DTOs: Auth
// ============================================================
namespace CMT.Core.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    int    UserId,
    string FullName,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string Role      // Admin | Staff
);

public record UpdateUserRequest(
    string FullName,
    string Email,
    string Role,
    bool   IsActive
);

public record UserDto(
    int    Id,
    string FullName,
    string Email,
    string Role,
    bool   IsActive,
    DateTime CreatedAt
);
