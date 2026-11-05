using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResultDto>> Register(RegisterDto dto, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(dto, ct);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Problem(result.Message, statusCode: result.Error switch
            {
                ServiceError.Conflict => StatusCodes.Status409Conflict,
                ServiceError.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            });
    }
}