using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClinicManagementSystem.API.Services
{
    public class DashboardService(ClinicDbContext context,TimeProvider timeProvider) : IDashboardService
    {
        public async Task<DashboardDto> GetDashboardAsync(int days, CancellationToken ct)
        {
            var doctors = await GetDoctorStatsAsync(ct);
            var appointments = await GetAppointmentStatsAsync(ct);
            var daily = await GetDailyBookingsAsync(days, ct);

            return new DashboardDto(doctors, appointments, daily);
        }
        private async Task<DoctorStatsDto> GetDoctorStatsAsync(CancellationToken ct)
        {
            var bySpecialization = await context.Doctors
                .AsNoTracking()
                .GroupBy(d => d.Specialization ?? "General practice")
                .OrderByDescending(g => g.Count())      // sort the groups first
                .ThenBy(g => g.Key)                     // tie-breaker, so the order is stable
                .Select(g => new SpecializationCountDto(g.Key, g.Count()))   // project LAST
                .ToListAsync(ct);

            var total = bySpecialization.Sum(x => x.Count);

            return new DoctorStatsDto(total, bySpecialization);
        }
        private async Task<IReadOnlyList<DailyBookingDto>> GetDailyBookingsAsync(int days, CancellationToken ct)
        {
            // 1. Decide the window in C#
            var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
            var firstDay = today.AddDays(-(days - 1));

            var from = firstDay.ToDateTime(TimeOnly.MinValue);                 // inclusive
            var toExclusive = today.AddDays(1).ToDateTime(TimeOnly.MinValue);  // exclusive

            // 2. Let SQL do the counting (only days that HAVE bookings come back)
            var rows = await context.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime < toExclusive)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .GroupBy(a => a.AppointmentDateTime.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            // 3. Convert in memory
            var countsByDay = rows.ToDictionary(r => DateOnly.FromDateTime(r.Day), r => r.Count);

            // 4. Build the full list of days; a missing day means zero
            return Enumerable.Range(0, days)
                .Select(offset => firstDay.AddDays(offset))
                .Select(day => new DailyBookingDto(day, countsByDay.GetValueOrDefault(day)))
                .ToList();
        }
        private async Task<AppointmentStatsDto> GetAppointmentStatsAsync(CancellationToken ct)
        {
            var counts = await context.Appointments
                .AsNoTracking()
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

            // A missing group means zero
            int Get(AppointmentStatus s) => counts.GetValueOrDefault(s);

            return new AppointmentStatsDto(
                Total: counts.Values.Sum(),
                Scheduled: Get(AppointmentStatus.Scheduled),
                Completed: Get(AppointmentStatus.Completed),
                Cancelled: Get(AppointmentStatus.Cancelled),
                NoShow: Get(AppointmentStatus.NoShow));
        }
    }
}