using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static ClinicManagementSystem.API.DTOs.DoctorAvailabilityDtos;

namespace ClinicManagementSystem.API.Entities
{
    public class DoctorAvailabilityService(ClinicDbContext context) : IDoctorAvailabilityService
    {
        public async Task<Result<DoctorScheduleDto>> GetScheduleAsync(int doctorId, CancellationToken ct)
        {
            if (!await DoctorExists(doctorId, ct))
                return Result<DoctorScheduleDto>.Fail(ResultStatus.NotFound, "Doctor not found.");

            var hours = await context.DoctorWorkingHours.AsNoTracking()
                .Where(x => x.DoctorId == doctorId)
                .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
                .Select(x => new TimeRangeDto(x.DayOfWeek, x.StartTime, x.EndTime))
                .ToListAsync(ct);

            var breaks = await context.DoctorBreaks.AsNoTracking()
                .Where(x => x.DoctorId == doctorId)
                .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
                .Select(x => new TimeRangeDto(x.DayOfWeek, x.StartTime, x.EndTime))
                .ToListAsync(ct);

            return Result<DoctorScheduleDto>.Ok(new DoctorScheduleDto(hours, breaks));
        }

        public async Task<Result<DoctorScheduleDto>> SaveScheduleAsync(
            int doctorId, DoctorScheduleDto dto, CancellationToken ct)
        {
            if (!await DoctorExists(doctorId, ct))
                return Result<DoctorScheduleDto>.Fail(ResultStatus.NotFound, "Doctor not found.");

            var error = ScheduleValidator.Validate(dto);
            if (error is not null)
                return Result<DoctorScheduleDto>.Fail(ResultStatus.Invalid, error);

            // Replace strategy: delete old rows + insert new ones, atomically.
            await using var tx = await context.Database.BeginTransactionAsync(ct);

            await context.DoctorWorkingHours.Where(x => x.DoctorId == doctorId).ExecuteDeleteAsync(ct);
            await context.DoctorBreaks.Where(x => x.DoctorId == doctorId).ExecuteDeleteAsync(ct);

            context.DoctorWorkingHours.AddRange(dto.WorkingHours.Select(r => new DoctorWorkingHour
            { DoctorId = doctorId, DayOfWeek = r.DayOfWeek, StartTime = r.StartTime, EndTime = r.EndTime }));
            context.DoctorBreaks.AddRange(dto.Breaks.Select(r => new DoctorBreak
            { DoctorId = doctorId, DayOfWeek = r.DayOfWeek, StartTime = r.StartTime, EndTime = r.EndTime }));

            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Result<DoctorScheduleDto>.Ok(dto);
        }

        public async Task<Result<IReadOnlyList<TimeOffDto>>> GetTimeOffsAsync(int doctorId, CancellationToken ct)
        {
            if (!await DoctorExists(doctorId, ct))
                return Result<IReadOnlyList<TimeOffDto>>.Fail(ResultStatus.NotFound, "Doctor not found.");

            var now = DateTime.Now; // keep the same clock convention as AppointmentDateTime
            IReadOnlyList<TimeOffDto> list = await context.DoctorTimeOffs.AsNoTracking()
                .Where(x => x.DoctorId == doctorId && x.EndDateTime >= now) // upcoming only
                .OrderBy(x => x.StartDateTime)
                .Select(x => new TimeOffDto(x.DoctorTimeOffId, x.StartDateTime, x.EndDateTime, x.Reason))
                .ToListAsync(ct);

            return Result<IReadOnlyList<TimeOffDto>>.Ok(list);
        }

        public async Task<Result<TimeOffDto>> AddTimeOffAsync(int doctorId, CreateTimeOffDto dto, CancellationToken ct)
        {
            if (!await DoctorExists(doctorId, ct))
                return Result<TimeOffDto>.Fail(ResultStatus.NotFound, "Doctor not found.");

            if (dto.EndDateTime <= dto.StartDateTime)
                return Result<TimeOffDto>.Fail(ResultStatus.Invalid, "End must be after start.");
            if (dto.EndDateTime <= DateTime.Now)
                return Result<TimeOffDto>.Fail(ResultStatus.Invalid, "Time off must end in the future.");
            if (dto.Reason?.Length > 200)
                return Result<TimeOffDto>.Fail(ResultStatus.Invalid, "Reason must be 200 characters or fewer.");

            var hasBookings = await context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.Status == AppointmentStatus.Scheduled &&
                a.AppointmentDateTime >= dto.StartDateTime &&
                a.AppointmentDateTime < dto.EndDateTime, ct);

            if (hasBookings)
                return Result<TimeOffDto>.Fail(ResultStatus.Conflict,
                    "There are scheduled appointments in this period. Cancel or reschedule them first.");

            var entity = new DoctorTimeOff
            {
                DoctorId = doctorId,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Reason = dto.Reason?.Trim()
            };
            context.DoctorTimeOffs.Add(entity);
            await context.SaveChangesAsync(ct);

            return Result<TimeOffDto>.Ok(
                new TimeOffDto(entity.DoctorTimeOffId, entity.StartDateTime, entity.EndDateTime, entity.Reason));
        }

        public async Task<Result<bool>> DeleteTimeOffAsync(int doctorId, int timeOffId, CancellationToken ct)
        {
            var deleted = await context.DoctorTimeOffs
                .Where(x => x.DoctorTimeOffId == timeOffId && x.DoctorId == doctorId)
                .ExecuteDeleteAsync(ct);

            return deleted == 0
                ? Result<bool>.Fail(ResultStatus.NotFound, "Time off not found.")
                : Result<bool>.Ok(true);
        }

        private Task<bool> DoctorExists(int doctorId, CancellationToken ct) =>
            context.Doctors.AnyAsync(d => d.DoctorId == doctorId, ct);
    }
}
