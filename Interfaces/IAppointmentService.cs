using ClinicManagementSystem.API.Common;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IAppointmentService
    {

        Task<PagedResult<AppointmentListItemDto>> GetAppointmentsAsync(int patientId, AppointmentScope scope, int pageNumber, int pageSize, CancellationToken ct);
        Task<AppointmentDetailsDto>GetByIdAsync(int patientId,int appointmentId,CancellationToken ct);

        Task<CancelOutcome>CancelAsync(int patientId,int appointmentId,CancellationToken ct);


        Task<ServiceResult<IReadOnlyList<SlotDto>>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct);
        Task<ServiceResult<AppointmentDto>> BookAsync(CreateAppointmentRequest request, CancellationToken ct);


    }
}
