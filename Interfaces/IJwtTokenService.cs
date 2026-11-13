using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.Interfaces;

public record AccessToken(string Value, DateTime ExpiresAtUtc);


public interface IJwtTokenService
{
    Task<AccessToken> Create(UserAccount account);
}

