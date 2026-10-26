namespace ClinicManagementSystem.API.Interfaces;

public interface IAppointmentReminderService
{
    /// Creates reminders that are due. Safe to call repeatedly. Returns how many appointments were processed.
    Task<int> SendDueRemindersAsync(CancellationToken ct);
}