using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.API.DTOs;

public record RegisterDto(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    DateOnly? DateOfBirth,
    [Required, RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")] string Gender,
    [Required, StringLength(20), Phone] string PhoneNumber,
    [Required, EmailAddress, StringLength(100)] string Email,
    [Required, StringLength(128, MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 128 characters.")] string Password,
    [StringLength(200)] string? Address);

public record LoginDto(
    [Required, EmailAddress, StringLength(100)] string Email,
    [Required, StringLength(128)] string Password);