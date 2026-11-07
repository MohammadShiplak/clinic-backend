namespace ClinicManagementSystem.API.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string SigningKey { get; init; } = "";      // from user-secrets / env var, NEVER appsettings.json
    public int AccessTokenMinutes { get; init; } = 30;
}