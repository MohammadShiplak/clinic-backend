using ClinicManagementSystem.API.Entities;

namespace ClinicManagementSystem.API.Common;

public static class AppRoles
{
    public const string Patient = nameof(UserRole.Patient);   // "Patient": stays in sync with the enum
    public const string Doctor = nameof(UserRole.Doctor);
    public const string Admin = nameof(UserRole.Admin);
}

public static class AppClaims
{
    public const string Role = "role";
    public const string PersonId = "person_id";
    public const string PatientId = "patient_id";
}