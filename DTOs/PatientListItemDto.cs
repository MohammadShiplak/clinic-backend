namespace ClinicManagementSystem.API.DTOs
{

        public record PatientListItemDto(
            int PatientId,
            string FullName,
            string PhoneNumber,
            string? Email,
            int? Age,
            bool IsActive);
    
}
