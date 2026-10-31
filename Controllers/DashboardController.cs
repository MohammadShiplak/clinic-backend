using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.API.Controllers
{
    // TODO(auth): add [Authorize(Roles = "Admin")] once JWT + roles exist.
    // This endpoint exposes business data. It must NOT ship to production without it.
    [Route("api/admin/dashboard")]
    [ApiController]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        private const int MinDays = 1;
        private const int MaxDays = 90;

        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DashboardDto>> GetDashboard(
            [FromQuery] int days = 7,
            CancellationToken ct = default)
        {
            if (days is < MinDays or > MaxDays)
                return Problem(
                    detail: $"'days' must be between {MinDays} and {MaxDays}.",
                    statusCode: StatusCodes.Status400BadRequest);

            return Ok(await dashboardService.GetDashboardAsync(days, ct));
        }
    }
}