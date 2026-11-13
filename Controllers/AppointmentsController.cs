using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.Patient)]
public class AppointmentsController(IAppointmentService service) : ControllerBase
{
    private const int MaxPageSize = 50;

    // Identity comes from the signed token, never from the URL or the body.
    private int PatientId => User.GetRequiredPatientId();

    [HttpGet("availability")]
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> GetAvailability(
        [FromQuery] int doctorId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var result = await service.GetAvailableSlotsAsync(doctorId, date, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error, result.Message);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Book(CreateAppointmentRequest request, CancellationToken ct)
    {
        var result = await service.BookAsync( request, ct);
        return result.IsSuccess
            ? Created($"/api/appointments/{result.Value!.AppointmentId}", result.Value)
            : ToProblem(result.Error, result.Message);
    }

    [HttpGet]
    public async Task<ActionResult<AppointmentDtos.PagedResult<AppointmentListItemDto>>> GetAppointments(
        [FromQuery] AppointmentScope scope = AppointmentScope.Upcoming,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(scope))
            return BadRequest("scope must be 'upcoming' or 'past'.");

        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest($"pageNumber must be at least 1 and pageSize between 1 and {MaxPageSize}.");

        return Ok(await service.GetAppointmentsAsync( scope, pageNumber, pageSize, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDetailsDto>> GetAppointment(int id, CancellationToken ct)
    {
        var dto = await service.GetByIdAsync( id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        return await service.CancelAsync( id, ct) switch
        {
            CancelOutcome.Cancelled => NoContent(),
            CancelOutcome.NotFound => NotFound(),
            CancelOutcome.NotEligible => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Appointment cannot be cancelled",
                Detail = "Only scheduled appointments at least 24 hours away can be cancelled."
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private ObjectResult ToProblem(ServiceError error, string? message) => error switch
    {
        ServiceError.NotFound => Problem(message, statusCode: StatusCodes.Status404NotFound),
        ServiceError.Conflict => Problem(message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(message, statusCode: StatusCodes.Status400BadRequest)
    };
}