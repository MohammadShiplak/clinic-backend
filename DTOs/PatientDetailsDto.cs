namespace ClinicManagementSystem.API.DTOs
{
    public record PatientDetailsDto(
       int PatientId,
       string FullName,
       DateOnly? DateOfBirth,
       string Gender,
       string PhoneNumber,
       string? Email,
       string? Address,
       bool IsActive);
}
