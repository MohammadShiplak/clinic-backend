using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicManagementSystem.API.Services;

public class AppointmentReminderService(
    ClinicDbContext context,
    INotificationService notifications,
    IOptions<AppointmentSettings> appointmentOptions,
    IOptions<ReminderSettings> reminderOptions,
    TimeProvider clock,
    ILogger<AppointmentReminderService> logger) : IAppointmentReminderService
{
    private const int BatchSize = 200;   // bound the work done per tick

    private readonly TimeZoneInfo _clinicTz =
        TimeZoneInfo.FindSystemTimeZoneById(appointmentOptions.Value.TimeZoneId);

    public async Task<int> SendDueRemindersAsync(CancellationToken ct)
    {
        var now = ClinicNow();
        var windowEnd = now.AddHours(reminderOptions.Value.LeadTimeHours);

        // Tracking is ON on purpose: we hand the Appointment entity to Notification.Appointment,
        // and a TRACKED entity is recognised by EF as "already in the database".
        var due = await context.Appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled
                     && a.AppointmentDateTime > now
                     && a.AppointmentDateTime <= windowEnd
                     && !context.Notifications.Any(n =>
                            n.AppointmentId == a.AppointmentId
                            && n.Type == NotificationType.AppointmentReminder))
            .OrderBy(a => a.AppointmentDateTime)
            .Take(BatchSize)
            .Select(a => new
            {
                Appointment = a,
                PatientPersonId = a.Patient.PersonId,
                PatientName = a.Patient.Person.Name,
                DoctorPersonId = a.Doctor.PersonId,
                DoctorName = a.Doctor.Person.Name
            })
            .ToListAsync(ct);

        if (due.Count == 0) return 0;

        foreach (var d in due)
        {
            notifications.AddAppointmentReminder(new AppointmentNotificationContext(
                d.Appointment,
                d.PatientPersonId, d.PatientName,
                d.DoctorPersonId, d.DoctorName));
        }

        try
        {
            await context.SaveChangesAsync(ct);   // all reminders in one transaction
            return due.Count;
        }
        catch (DbUpdateException ex)
        {
            // e.g. the unique index caught a duplicate. Nothing was saved (rolled back);
            // next tick re-queries and the already-existing reminders are excluded.
            logger.LogWarning(ex, "Saving reminders failed; will retry on the next tick.");
            return 0;
        }
    }

    private DateTime ClinicNow() =>
        TimeZoneInfo.ConvertTimeFromUtc(clock.GetUtcNow().UtcDateTime, _clinicTz);
}