using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDtos.PagedResult<AppointmentListItemDto>> GetAppointmentsAsync(
            AppointmentScope scope, int pageNumber, int pageSize, CancellationToken ct);

        Task<AppointmentDetailsDto?> GetByIdAsync(int appointmentId, CancellationToken ct);

        Task<CancelOutcome> CancelAsync(int appointmentId, CancellationToken ct);

        Task<ServiceResult<IReadOnlyList<SlotDto>>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct);
        Task<ServiceResult<AppointmentDto>> BookAsync(CreateAppointmentRequest request, CancellationToken ct);
    }
}
