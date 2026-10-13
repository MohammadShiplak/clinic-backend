using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IDoctorService
    {
        Task<IReadOnlyList<DoctorListItemDto>> GetDoctorsAsync(string? search, string? specialization, CancellationToken ct);
        Task<IReadOnlyList<string>> GetSpecializationAsync(CancellationToken ct);
    }
}
