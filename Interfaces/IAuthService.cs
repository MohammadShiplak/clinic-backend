using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<RegisterResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct);
}