// Interfaces/IPatientService.cs
using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces;

public enum CreatePatientOutcome { Created, Duplicate }
// Interfaces/IPatientService.cs: add next to CreatePatientOutcome
public enum DeactivatePatientOutcome { Deactivated, NotFound, HasUpcomingAppointments }
public record CreatePatientResult(CreatePatientOutcome Outcome, PatientDetailsDto? Patient);

public interface IPatientService
{
    Task<PagedResult<PatientListItemDto>> GetPatientsAsync(
        string? search, bool includeInactive, int pageNumber, int pageSize, CancellationToken ct);

    Task<PatientDetailsDto?> GetPatientAsync(int patientId, CancellationToken ct);

    Task<CreatePatientResult> CreateAsync(CreatePatientDto dto, CancellationToken ct);

    Task<PatientDetailsDto?> UpdateAsync(int patientId, UpdatePatientDto dto, CancellationToken ct);



    Task<bool> ReactivateAsync(int patientId, CancellationToken ct);
    // Interfaces/IPatientService.cs  (add this method)
    Task<PagedResult<PatientAppointmentDto>?> GetAppointmentHistoryAsync(
        int patientId, PatientAppointmentsQuery query, CancellationToken ct);
    Task<DeactivatePatientOutcome> DeactivateAsync(int patientId, CancellationToken ct);

}