// Controllers/PatientsController.cs
using System.ComponentModel.DataAnnotations;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin,Receptionist")]   // enable once JWT auth is configured (see section 6)
public class PatientsController(IPatientService patientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatientListItemDto>>> GetPatients(
        [FromQuery, StringLength(100)] string? search,
        [FromQuery] bool includeInactive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await patientService.GetPatientsAsync(search, includeInactive, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDetailsDto>> GetPatient(int id, CancellationToken ct)
    {
        var patient = await patientService.GetPatientAsync(id, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<PatientDetailsDto>> CreatePatient(CreatePatientDto dto, CancellationToken ct)
    {
        var result = await patientService.CreateAsync(dto, ct);

        if (result.Outcome == CreatePatientOutcome.Duplicate)
        {
            return Problem(
                title: "Patient already exists",
                detail: "A patient with the same name, phone number and date of birth is already registered.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return CreatedAtAction(nameof(GetPatient), new { id = result.Patient!.PatientId }, result.Patient);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDetailsDto>> UpdatePatient(int id, UpdatePatientDto dto, CancellationToken ct)
    {
        var patient = await patientService.UpdateAsync(id, dto, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivatePatient(int id, CancellationToken ct)
    {
        var outcome = await patientService.DeactivateAsync(id, ct);

        if (outcome == DeactivatePatientOutcome.NotFound)
            return NotFound();

        if (outcome == DeactivatePatientOutcome.HasUpcomingAppointments)
        {
            return Problem(
                title: "Patient has upcoming appointments",
                detail: "Cancel the patient's scheduled appointments before deactivating.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return NoContent();
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> ReactivatePatient(int id, CancellationToken ct) =>
        await patientService.ReactivateAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("{id:int}/appointments")]
    public async Task<ActionResult<PagedResult<PatientAppointmentDto>>> GetAppointmentHistory(
        int id, [FromQuery] PatientAppointmentsQuery query, CancellationToken ct)
    {
        var history = await patientService.GetAppointmentHistoryAsync(id, query, ct);
        return history is null ? NotFound() : Ok(history);
    }
}