using System.Security.Claims;

namespace ClinicManagementSystem.API.Common;

public static class ClaimsPrincipalExtensions
{
    public static int GetRequiredPersonId(this ClaimsPrincipal user) =>
        GetRequiredInt(user, AppClaims.PersonId);

    public static int GetRequiredPatientId(this ClaimsPrincipal user) =>
        GetRequiredInt(user, AppClaims.PatientId);

    private static int GetRequiredInt(ClaimsPrincipal user, string claimType) =>
        int.TryParse(user.FindFirstValue(claimType), out var id)
            ? id
            : throw new InvalidOperationException($"Claim '{claimType}' is missing or invalid.");
}