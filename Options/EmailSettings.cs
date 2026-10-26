namespace ClinicManagementSystem.API.Options;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Provider { get; init; } = "Log";       // "Log" (dev) or "Smtp" (production)

    // SMTP connection (only used when Provider = "Smtp")
    public string Host { get; init; } = "";
    public int Port { get; init; } = 587;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";          // NEVER put this in appsettings.json (see below)

    public string FromAddress { get; init; } = "no-reply@clinic.example";
    public string FromName { get; init; } = "Clinic";

    // Dispatch policy
    public int DispatchIntervalSeconds { get; init; } = 30;
    public int MaxAttempts { get; init; } = 5;
}