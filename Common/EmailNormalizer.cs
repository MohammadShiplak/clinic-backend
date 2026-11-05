namespace ClinicManagementSystem.API.Common;

public static class EmailNormalizer
{
    // The one and only place that decides what "the same email" means.
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}