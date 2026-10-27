using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.API.DTOs
{
    public record CreatePatientDto(
       [Required, StringLength(100, MinimumLength = 2)] string FullName,
       DateOnly? DateOfBirth,
       [Required, RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")] string Gender,
       [Required, StringLength(20), Phone] string PhoneNumber,
       [EmailAddress, StringLength(100)] string? Email,
       [StringLength(200)] string? Address);
}
