using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ClinicManagementSystem.API.DTOs.DoctorAvailabilityDtos;

[ApiController]
[Route("api/doctors/{doctorId:int}")]
public class DoctorAvailabilityController(IDoctorAvailabilityService service) : ControllerBase
{
    [HttpGet("schedule")]
    public async Task<ActionResult<DoctorScheduleDto>> GetSchedule(int doctorId, CancellationToken ct) =>
        Map(await service.GetScheduleAsync(doctorId, ct));

    [HttpPut("schedule")]
    public async Task<ActionResult<DoctorScheduleDto>> SaveSchedule(
        int doctorId, DoctorScheduleDto dto, CancellationToken ct) =>
        Map(await service.SaveScheduleAsync(doctorId, dto, ct));

    [HttpGet("time-off")]
    public async Task<ActionResult<IReadOnlyList<TimeOffDto>>> GetTimeOffs(int doctorId, CancellationToken ct) =>
        Map(await service.GetTimeOffsAsync(doctorId, ct));

    [HttpPost("time-off")]
    public async Task<ActionResult<TimeOffDto>> AddTimeOff(
        int doctorId, CreateTimeOffDto dto, CancellationToken ct) =>
        Map(await service.AddTimeOffAsync(doctorId, dto, ct));

    [HttpDelete("time-off/{timeOffId:int}")]
    public async Task<IActionResult> DeleteTimeOff(int doctorId, int timeOffId, CancellationToken ct)
    {
        var r = await service.DeleteTimeOffAsync(doctorId, timeOffId, ct);
        return r.Status == ResultStatus.Ok ? NoContent() : Problem(r);
    }

    private ActionResult<T> Map<T>(Result<T> r) =>
        r.Status == ResultStatus.Ok ? Ok(r.Value) : Problem(r);

    private ObjectResult Problem<T>(Result<T> r) => Problem(
        detail: r.Error,
        statusCode: r.Status switch
        {
            ResultStatus.NotFound => 404,
            ResultStatus.Invalid => 400,
            ResultStatus.Conflict => 409,
            _ => 500
        });
}