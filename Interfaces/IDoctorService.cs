using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IDoctorService
    {
        Task<PagedResult<DoctorListItemDto>> GetDoctorsAsync(DoctorQueryParameters query, CancellationToken ct);
        Task<IReadOnlyList<string>> GetSpecializationAsync(CancellationToken ct);
        Task<DoctorDetailsDto?> GetDoctorByIdAsync(int id, CancellationToken ct);
    }
}
