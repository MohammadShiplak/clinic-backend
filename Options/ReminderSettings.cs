namespace ClinicManagementSystem.API.Options;

public class ReminderSettings
{
    public const string SectionName = "Reminders";

    public int LeadTimeHours { get; init; } = 24;        // remind this long before the appointment
    public int CheckIntervalMinutes { get; init; } = 5;  // how often the worker wakes up
}