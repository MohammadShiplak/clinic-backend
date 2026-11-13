using ClinicManagementSystem.API.Auth;
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
    public class AppointmentService(
        ClinicDbContext context,
        IOptions<AppointmentSettings> options,
        TimeProvider timeProvider,
        INotificationService notifications,
        ICurrentUser currentUser) : IAppointmentService
    {
        private readonly AppointmentSettings _s = options.Value;
        private readonly TimeZoneInfo _clinicTz = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

        // AppointmentDateTime is CLINIC-LOCAL, so every "now" is clinic-local too.
        private DateTime ClinicNow() =>
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _clinicTz).DateTime;

        private bool IsStaff => currentUser.IsAdmin || currentUser.DoctorId is not null;

        // ONE place that defines which appointments the caller may see.
        // Every read and write below starts from this query.
        private IQueryable<Appointment> VisibleAppointments()
        {
            var query = context.Appointments.AsQueryable();

            if (currentUser.IsAdmin) return query;
            if (currentUser.DoctorId is { } doctorId) return query.Where(a => a.DoctorId == doctorId);
            if (currentUser.PatientId is { } patientId) return query.Where(a => a.PatientId == patientId);

            return query.Where(_ => false);   // unknown user shape: fail closed
        }

        // Patients keep the 24-hour notice rule. Staff may cancel any future scheduled appointment.
        private bool CanCancel(AppointmentStatus status, DateTime when, DateTime now) =>
            IsStaff
                ? status == AppointmentStatus.Scheduled && when > now
                : Appointment.IsCancellable(status, when, now);

        // ======================= BOOKING =======================

        public async Task<ServiceResult<AppointmentDto>> BookAsync(CreateAppointmentRequest request, CancellationToken ct)
        {
            // Identity from the TOKEN. A body-supplied patient id would be mass assignment.
            if (currentUser.PatientId is not { } patientId)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.Forbidden, "Only patients can book appointments.");

            var requested = DateTime.SpecifyKind(request.AppointmentDateTime, DateTimeKind.Unspecified);
            var date = DateOnly.FromDateTime(requested);

            var validSlots = SlotGenerator.Generate(
                date, _s.WorkdayStart, _s.WorkdayEnd, _s.SlotMinute, new HashSet<DateTime>(), ClinicNow());

            if (!validSlots.Contains(requested))
                return ServiceResult<AppointmentDto>.Fail(
                    ServiceError.Validation, "The selected time is not a valid bookable slot.");

            var doctor = await context.Doctors.AsNoTracking()
                .Where(d => d.DoctorId == request.DoctorId)
                .Select(d => new { d.PersonId, d.Person.Name })
                .FirstOrDefaultAsync(ct);

            if (doctor is null)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Doctor not found.");

            var patient = await context.Patients.AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .Select(p => new { p.PersonId, p.Person.Name, p.IsActive })
                .FirstOrDefaultAsync(ct);

            if (patient is null)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Patient not found.");

            if (!patient.IsActive)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.Forbidden, "This patient account is inactive.");

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = patientId,
                AppointmentDateTime = requested,
                Status = AppointmentStatus.Scheduled
            };

            context.Appointments.Add(appointment);

            notifications.AddAppointmentBooked(new AppointmentNotificationContext(
                appointment,
                patient.PersonId, patient.Name,
                doctor.PersonId, doctor.Name));

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return ServiceResult<AppointmentDto>.Fail(
                    ServiceError.Conflict, "This slot was just booked by someone else. Please choose another time.");
            }

            return ServiceResult<AppointmentDto>.Ok(new AppointmentDto(
                appointment.AppointmentId,
                appointment.DoctorId,
                doctor.Name,
                appointment.AppointmentDateTime,
                appointment.Status.ToString()));
        }

        public async Task<ServiceResult<IReadOnlyList<SlotDto>>> GetAvailableSlotsAsync(
            int doctorId, DateOnly date, CancellationToken ct)
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

        private async Task<HashSet<DateTime>> GetBookedTimesAsync(int doctorId, DateOnly date, CancellationToken ct)
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);

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

        // ======================= APPOINTMENT MANAGEMENT =======================

        public async Task<AppointmentDtos.PagedResult<AppointmentListItemDto>> GetAppointmentsAsync(
            AppointmentScope scope, int pageNumber, int pageSize, CancellationToken ct)
        {
            var now = ClinicNow();

            var query = VisibleAppointments().AsNoTracking();

            query = scope == AppointmentScope.Upcoming
                ? query.Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentDateTime >= now)
                       .OrderBy(a => a.AppointmentDateTime).ThenBy(a => a.AppointmentId)
                : query.Where(a => a.Status != AppointmentStatus.Scheduled || a.AppointmentDateTime < now)
                       .OrderByDescending(a => a.AppointmentDateTime).ThenByDescending(a => a.AppointmentId);

            var totalCount = await query.CountAsync(ct);

            var rows = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.DoctorId,
                    DoctorName = a.Doctor.Person.Name,
                    a.Doctor.Specialization,
                    a.Doctor.ProfileImageUrl,
                    a.AppointmentDateTime,
                    a.Status
                })
                .ToListAsync(ct);

            var items = rows.Select(r => new AppointmentListItemDto(
                r.AppointmentId,
                r.DoctorId,
                r.DoctorName,
                r.Specialization,
                r.ProfileImageUrl,
                r.AppointmentDateTime,
                r.Status,
                CanCancel(r.Status, r.AppointmentDateTime, now))).ToList();

            return new AppointmentDtos.PagedResult<AppointmentListItemDto>(items, pageNumber, pageSize, totalCount);
        }

        public async Task<AppointmentDetailsDto?> GetByIdAsync(int appointmentId, CancellationToken ct)
        {
            var now = ClinicNow();

            // Someone else's appointment is not in VisibleAppointments(), so it simply isn't found.
            var row = await VisibleAppointments()
                .AsNoTracking()
                .Where(a => a.AppointmentId == appointmentId)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.DoctorId,
                    DoctorName = a.Doctor.Person.Name,
                    a.Doctor.Specialization,
                    a.Doctor.ProfileImageUrl,
                    a.AppointmentDateTime,
                    a.Status,
                    a.MedicalRecordId,
                    VisitDescription = a.MedicalRecord != null ? a.MedicalRecord.VisitDescription : null,
                    Diagnosis = a.MedicalRecord != null ? a.MedicalRecord.Diagnosis : null,
                    AdditionalNotes = a.MedicalRecord != null ? a.MedicalRecord.AdditionalNotes : null,
                    PaymentDate = a.Payment != null ? (DateOnly?)a.Payment.PaymentDate : null,
                    PaymentMethod = a.Payment != null ? a.Payment.PaymentMethod : null,
                    AmountPaid = a.Payment != null ? (decimal?)a.Payment.AmountPaid : null
                })
                .FirstOrDefaultAsync(ct);

            if (row is null) return null;

            var prescriptions = new List<PrescriptionDto>();
            if (row.MedicalRecordId is int recordId)
            {
                prescriptions = await context.Prescriptions
                    .AsNoTracking()
                    .Where(p => p.MedicalRecordId == recordId)
                    .OrderBy(p => p.StartDate)
                    .Select(p => new PrescriptionDto(
                        p.MedicationName, p.Dosage, p.Frequency,
                        p.StartDate, p.EndDate, p.SpecialInstructions))
                    .ToListAsync(ct);
            }

            var payment = row.PaymentDate is DateOnly paidOn && row.AmountPaid is decimal amount
                ? new PaymentDto(paidOn, row.PaymentMethod, amount)
                : null;

            return new AppointmentDetailsDto(
                row.AppointmentId,
                row.DoctorId,
                row.DoctorName,
                row.Specialization,
                row.ProfileImageUrl,
                row.AppointmentDateTime,
                row.Status,
                CanCancel(row.Status, row.AppointmentDateTime, now),
                row.VisitDescription,
                row.Diagnosis,
                row.AdditionalNotes,
                prescriptions,
                payment);
        }

        public async Task<CancelOutcome> CancelAsync(int appointmentId, CancellationToken ct)
        {
            // Tracked on purpose (we modify it), but still scoped to what the caller may see.
            var appointment = await VisibleAppointments()
                .Include(a => a.Patient).ThenInclude(p => p.Person)
                .Include(a => a.Doctor).ThenInclude(d => d.Person)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);

            // Not yours looks exactly like not existing: no id probing.
            if (appointment is null) return CancelOutcome.NotFound;

            var now = ClinicNow();
            bool cancelled;

            if (IsStaff)
            {
                cancelled = appointment.Status == AppointmentStatus.Scheduled
                            && appointment.AppointmentDateTime > now;
                if (cancelled) appointment.Status = AppointmentStatus.Cancelled;
            }
            else
            {
                cancelled = appointment.TryCancel(now);   // patients: 24-hour rule
            }

            if (!cancelled) return CancelOutcome.NotEligible;

            // Notify everyone except whoever cancelled. Id 0 matches no one, so both parties hear about an admin cancel.
            var cancelledByPersonId =
                currentUser.IsAdmin ? 0
                : currentUser.DoctorId is not null ? appointment.Doctor.PersonId
                : appointment.Patient.PersonId;

            notifications.AddAppointmentCancelled(
                new AppointmentNotificationContext(
                    appointment,
                    appointment.Patient.PersonId, appointment.Patient.Person.Name,
                    appointment.Doctor.PersonId, appointment.Doctor.Person.Name),
                cancelledByPersonId);

            await context.SaveChangesAsync(ct);
            return CancelOutcome.Cancelled;
        }
    }
}