using ClinicManagementSystem.API.DTOs;

namespace ClinicManagementSystem.API.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(int days, CancellationToken ct);
    }
}