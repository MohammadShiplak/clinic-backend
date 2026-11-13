using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicManagementSystem.API.Auth;
using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagementSystem.API.Services;

public class JwtTokenService(
    IOptions<JwtSettings> options,
    TimeProvider clock,
    ClinicDbContext context) : IJwtTokenService
{
    private readonly JwtSettings _s = options.Value;

    // Build signing credentials once when the service is created.
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.Value.SigningKey)),
        SecurityAlgorithms.HmacSha256);

    private readonly JwtSecurityTokenHandler _handler = new();

    public async Task<AccessToken> Create(UserAccount account)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var expires = now.AddMinutes(_s.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,
                account.UserAccountId.ToString()),

            new(JwtRegisteredClaimNames.Email,
                account.Email),

            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(AppClaims.Role,
                account.Role.ToString()),

            new("person_id",
                account.PersonId.ToString())
        };

        var ids = await context.People
            .AsNoTracking()
            .Where(p => p.PersonId == account.PersonId)
            .Select(p => new
            {
                PatientId = (int?)p.Patient!.PatientId,
                DoctorId = (int?)p.Doctor!.DoctorId
            })
            .SingleAsync();

        if (ids.PatientId is { } patientId)
        {
            claims.Add(new Claim(
                AppClaimTypes.PatientId,
                patientId.ToString()));
        }

        if (ids.DoctorId is { } doctorId)
        {
            claims.Add(new Claim(
                AppClaimTypes.DoctorId,
                doctorId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _s.Issuer,
            audience: _s.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _credentials);

        var tokenValue = _handler.WriteToken(token);

        return new AccessToken(tokenValue, expires);
    }
}