using ClinicManagementSystem.API.Common;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IAppointmentService
    {
        Task<ServiceResult<IReadOnlyList<SlotDto>>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct);
        Task<ServiceResult<AppointmentDto>> BookAsync(CreateAppointmentRequest request, CancellationToken ct);


    }
}
