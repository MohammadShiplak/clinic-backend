// Controllers/AppointmentsController.cs
using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController(IAppointmentService service) : ControllerBase
{
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
        var result = await service.BookAsync(request, ct);
        return result.IsSuccess
            ? Created($"/api/appointments/{result.Value!.AppointmentId}", result.Value)
            : ToProblem(result.Error, result.Message);
    }

    private ObjectResult ToProblem(ServiceError error, string? message) => error switch
    {
        ServiceError.NotFound => Problem(message, statusCode: StatusCodes.Status404NotFound),
        ServiceError.Conflict => Problem(message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(message, statusCode: StatusCodes.Status400BadRequest)
    };
}