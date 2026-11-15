namespace ClinicManagementSystem.API.Options;

public class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public LimitRule Global { get; init; } = new() { PermitLimit = 100, WindowSeconds = 60 };
    public LimitRule Auth { get; init; } = new() { PermitLimit = 5, WindowSeconds = 60 };
    public LimitRule Booking { get; init; } = new() { PermitLimit = 10, WindowSeconds = 60 };
}

public class LimitRule
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
}