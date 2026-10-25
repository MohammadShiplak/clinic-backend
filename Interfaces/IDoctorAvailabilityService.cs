using static ClinicManagementSystem.API.DTOs.DoctorAvailabilityDtos;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IDoctorAvailabilityService
    {
        Task<Result<DoctorScheduleDto>> GetScheduleAsync(int doctorId, CancellationToken ct);
        Task<Result<DoctorScheduleDto>> SaveScheduleAsync(int doctorId, DoctorScheduleDto dto, CancellationToken ct);
        Task<Result<IReadOnlyList<TimeOffDto>>> GetTimeOffsAsync(int doctorId, CancellationToken ct);
        Task<Result<TimeOffDto>> AddTimeOffAsync(int doctorId, CreateTimeOffDto dto, CancellationToken ct);
        Task<Result<bool>> DeleteTimeOffAsync(int doctorId, int timeOffId, CancellationToken ct);
    }
}
