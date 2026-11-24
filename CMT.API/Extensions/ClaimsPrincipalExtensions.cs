// ============================================================
//  CMT.API – ClaimsPrincipal extension helpers
//  Extracts typed values from the JWT token claims.
// ============================================================
using System.Security.Claims;

namespace CMT.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the authenticated user's Id from the JWT 'userId' claim.</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue("userId")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found in token.");
        return int.Parse(claim);
    }

    /// <summary>Returns the authenticated user's role.</summary>
    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.GetRole() == "Admin";
}
