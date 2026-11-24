// ============================================================
//  CMT.Infrastructure – JWT Token Helper
// ============================================================
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMT.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CMT.Infrastructure.Helpers;

public class JwtTokenHelper
{
    private readonly IConfiguration _config;

    public JwtTokenHelper(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var jwtSection  = _config.GetSection("JwtSettings");
        var secretKey   = jwtSection["SecretKey"]!;
        var issuer      = jwtSection["Issuer"]!;
        var audience    = jwtSection["Audience"]!;
        var expiryHours = int.Parse(jwtSection["ExpiryHours"] ?? "10");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt   = DateTime.UtcNow.AddHours(expiryHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,               user.FullName),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim("userId",                      user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
