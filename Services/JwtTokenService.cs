using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagementSystem.API.Services;

public class JwtTokenService(IOptions<JwtSettings> options, TimeProvider clock) : IJwtTokenService
{
    private readonly JwtSettings _s = options.Value;

    // Built once: the key bytes never change while the app runs.
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey)),
        SecurityAlgorithms.HmacSha256);

    private readonly JwtSecurityTokenHandler _handler = new();

    public AccessToken Create(UserAccount account)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_s.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.UserAccountId.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),   // unique token id (useful for revocation later)
            new(ClaimTypes.Role, account.Role.ToString()),
            new("person_id", account.PersonId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _s.Issuer,
            audience: _s.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _credentials);

        return new AccessToken(_handler.WriteToken(token), expires);
    }
}