namespace ClinicManagementSystem.API.DTOs
{
    public record DoctorQualificationDto(string Title, string? Institution, int? YearAwarded);

    public record DoctorDetailsDto(int DoctorId, string FullName, string? Specialization, string? ProfileImageUrl, int? YearsOfExperience, string? Biography, string? ClinicRoom, IReadOnlyList<DoctorQualificationDto> Qualifications);

}