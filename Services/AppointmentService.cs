using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static ClinicManagementSystem.API.DTOs.AppointmentDtos;

namespace ClinicManagementSystem.API.Services
{
    public class AppointmentService(ClinicDbContext context, IOptions<AppointmentSettings> options, TimeProvider timeProvider) : IAppointmentService
    {

        private readonly AppointmentSettings _s = options.Value;
        private readonly TimeZoneInfo _clinicTz = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);






        public async Task<ServiceResult<AppointmentDto>> BookAsync(AppointmentDtos.CreateAppointmentRequest request, CancellationToken ct)
        {
            var requested = DateTime.SpecifyKind(request.AppointmentDateTime, DateTimeKind.Unspecified);

            var date = DateOnly.FromDateTime(requested);

            var validateSlots = SlotGenerator.Generate
                (date, _s.WorkdayStart, _s.WorkdayEnd, _s.SlotMinute, new HashSet<DateTime>(), ClinicNow());

            if (!validateSlots.Contains(requested))
            {
                return ServiceResult<AppointmentDto>.Fail(ServiceError.Validation, "The selected time is not a valid bookable slot.");
            }

            var doctorName = await context.Doctors.AsNoTracking()
                .Where(d => d.DoctorId == request.DoctorId)
                .Select(d => d.Person.Name)
                .FirstOrDefaultAsync(ct);

            if (doctorName is null)
            {
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Doctor Not Found");
            }

            if (!await context.Patients.AnyAsync(p => p.PatientId == request.PatientId, ct))
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "patient not found");

            var appointment = new Appointment
            {

                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                AppointmentDateTime = requested,
                Status = AppointmentStatus.Scheduled
            };

            context.Appointments.Add(appointment);

            try
            {
                await context.SaveChangesAsync(ct);

            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return ServiceResult<AppointmentDto>.Fail(ServiceError.Conflict,
                    "This slot was just booked by someone else. Olease choose another time.");
            }

            return ServiceResult<AppointmentDto>.Ok(new AppointmentDto
                (appointment.AppointmentId, appointment.DoctorId, doctorName,
                appointment.AppointmentDateTime, appointment.Status.ToString()));
        }
        private DateTime ClinicNow() =>
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _clinicTz).DateTime;

        private async Task<HashSet<DateTime>> GetBookedTimesAsync(int doctorId, DateOnly date, CancellationToken ct)
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);

            // Range comparison (not a.AppointmentDateTime.Date == ...) so SQL can use the index
            var times = await context.Appointments.AsNoTracking()
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDateTime >= dayStart
                         && a.AppointmentDateTime < dayEnd
                         && a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.AppointmentDateTime)
                .ToListAsync(ct);

            return times.ToHashSet();
        }

        private static bool IsUniqueViolation(DbUpdateException ex) =>
                ex.InnerException is SqlException { Number: 2601 or 2627 };
        public async  Task<ServiceResult<IReadOnlyList<SlotDto>>> GetAvailableSlotsAsync(int doctorId, DateOnly date, CancellationToken ct)
            {
            if (!await context.Doctors.AnyAsync(d => d.DoctorId == doctorId, ct))
                return ServiceResult<IReadOnlyList<SlotDto>>.Fail(ServiceError.NotFound, "Doctor not found.");

            var booked = await GetBookedTimesAsync(doctorId, date, ct);
            var starts = SlotGenerator.Generate(
                date, _s.WorkdayStart, _s.WorkdayEnd, _s.SlotMinute, booked, ClinicNow());

            var step = TimeSpan.FromMinutes(_s.SlotMinute);
            IReadOnlyList<SlotDto> slots = starts.Select(s => new SlotDto(s, s + step)).ToList();
            return ServiceResult<IReadOnlyList<SlotDto>>.Ok(slots);
        }
        }
    

    }

