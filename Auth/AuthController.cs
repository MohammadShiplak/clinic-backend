using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResultDto>> Register(RegisterDto dto, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(dto, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error, result.Message);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto dto, CancellationToken ct)
    {
        var result = await auth.LoginAsync(dto, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error, result.Message);
    }

    private ObjectResult ToProblem(ServiceError error, string? message) => error switch
    {
        ServiceError.Conflict => Problem(message, statusCode: StatusCodes.Status409Conflict),
        ServiceError.Unauthorized => Problem(message, statusCode: StatusCodes.Status401Unauthorized),
        ServiceError.NotFound => Problem(message, statusCode: StatusCodes.Status404NotFound),
        _ => Problem(message, statusCode: StatusCodes.Status400BadRequest)
    };
}