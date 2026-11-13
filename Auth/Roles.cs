// Auth/Roles.cs
namespace ClinicManagementSystem.API.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Patient = "Patient";
}

public static class AppClaimTypes
{
    public const string PatientId = "patient_id";
    public const string DoctorId = "doctor_id";
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string StaffOnly = "StaffOnly";
    public const string PatientOnly = "PatientOnly";
    public const string DoctorSelfOrAdmin = "DoctorSelfOrAdmin";   // new
}