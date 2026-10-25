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
        TimeProvider timeProvider,INotificationService notifications) : IAppointmentService
    {
        private readonly AppointmentSettings _s = options.Value;
        private readonly TimeZoneInfo _clinicTz = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

        // ---------------------------------------------------------------
        // Time rule for the WHOLE service:
        // AppointmentDateTime is stored as CLINIC-LOCAL time (Kind = Unspecified).
        // So every "now" we compare against it must also be clinic-local: ClinicNow().
        // ---------------------------------------------------------------
        private DateTime ClinicNow() =>
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _clinicTz).DateTime;

        // ======================= BOOKING =======================

        public async Task<ServiceResult<AppointmentDto>> BookAsync(CreateAppointmentRequest request, CancellationToken ct)
        {
            var requested = DateTime.SpecifyKind(request.AppointmentDateTime, DateTimeKind.Unspecified);
            var date = DateOnly.FromDateTime(requested);

            // Never trust the client: re-generate the day's slots and check the requested one is in them.
            var validSlots = SlotGenerator.Generate(
                date, _s.WorkdayStart, _s.WorkdayEnd, _s.SlotMinute, new HashSet<DateTime>(), ClinicNow());

            if (!validSlots.Contains(requested))
                return ServiceResult<AppointmentDto>.Fail(
                    ServiceError.Validation, "The selected time is not a valid bookable slot.");

            var doctor = await context.Doctors.AsNoTracking()
                .Where(d => d.DoctorId == request.DoctorId)
                .Select(d => new {d.PersonId,d.Person.Name })
                .FirstOrDefaultAsync(ct);

            if (doctor is null)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Doctor not found.");


            var patient = await context.Patients.AsNoTracking()
    .Where(p => p.PatientId == request.PatientId)
    .Select(p => new { p.PersonId, p.Person.Name })
    .FirstOrDefaultAsync(ct);


            if (patient is null)
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Patient not found.");



            if (!await context.Patients.AnyAsync(p => p.PatientId == request.PatientId, ct))
                return ServiceResult<AppointmentDto>.Fail(ServiceError.NotFound, "Patient not found.");

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
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
                // The filtered unique index (DoctorId + time, ignoring cancelled) caught a race.
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

            // Range comparison (not a.AppointmentDateTime.Date == ...) so SQL can use the index.
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
            int patientId, AppointmentScope scope, int pageNumber, int pageSize, CancellationToken ct)
        {
            var now = ClinicNow(); // FIX: was UTC

            // Security: EVERY query is scoped to the patient. Never trust an id alone.
            var query = context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId);

            // Upcoming = still scheduled AND in the future. Everything else is history,
            // including a "Scheduled" appointment whose time passed and nobody closed out.
            // The second ordering key keeps paging stable when two rows share a time.
            query = scope == AppointmentScope.Upcoming
                ? query.Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentDateTime >= now)
                       .OrderBy(a => a.AppointmentDateTime).ThenBy(a => a.AppointmentId)
                : query.Where(a => a.Status != AppointmentStatus.Scheduled || a.AppointmentDateTime < now)
                       .OrderByDescending(a => a.AppointmentDateTime).ThenByDescending(a => a.AppointmentId);

            var totalCount = await query.CountAsync(ct);

            // Project only the columns the card needs (no Include, no tracking).
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
                r.AppointmentDateTime, // FIX: clinic-local, sent without "Z"
                r.Status,
                Appointment.IsCancellable(r.Status, r.AppointmentDateTime, now))).ToList();

            return new AppointmentDtos.PagedResult<AppointmentListItemDto>(items, pageNumber, pageSize, totalCount);
        }

        // FIX: nullable return type, because we do return null when nothing matches.
        public async Task<AppointmentDetailsDto?> GetByIdAsync(int patientId, int appointmentId, CancellationToken ct)
        {
            var now = ClinicNow(); // FIX: was UTC

            var row = await context.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentId == appointmentId && a.PatientId == patientId)
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

            // Second small query instead of one giant join: simpler SQL, no row duplication.
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
                row.AppointmentDateTime, // FIX: clinic-local, sent without "Z"
                row.Status,
                Appointment.IsCancellable(row.Status, row.AppointmentDateTime, now),
                row.VisitDescription,
                row.Diagnosis,
                row.AdditionalNotes,
                prescriptions,
                payment);
        }

        public async Task<CancelOutcome> CancelAsync(int patientId, int appointmentId, CancellationToken ct)
        {
            // Tracked on purpose: we are about to modify this row.
            var appointment = await context.Appointments
          .Include(a => a.Patient).ThenInclude(p => p.Person)
          .Include(a => a.Doctor).ThenInclude(d => d.Person)
          .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patientId, ct);

            // Someone else's appointment looks exactly like a missing one (no id probing).
            if (appointment is null) return CancelOutcome.NotFound;

            if (!appointment.TryCancel(ClinicNow())) // FIX: was UTC
                return CancelOutcome.NotEligible;

            notifications.AddAppointmentCancelled(
       new AppointmentNotificationContext(
           appointment,
           appointment.Patient.PersonId, appointment.Patient.Person.Name,
           appointment.Doctor.PersonId, appointment.Doctor.Person.Name),
       cancelledByPersonId: appointment.Patient.PersonId);


            await context.SaveChangesAsync(ct);
            return CancelOutcome.Cancelled;
        }

        // AsUtc() is deleted: stamping clinic-local times as UTC was the bug.
    }
}