namespace ClinicManagementSystem.API.DTOs
{
    public record DoctorListItemDto
    (
       int DoctorId,
       string FullName,
       string? Specialization,
       string? ProfileImageUrl,
       int?  YearsOfExperience
        );
}
