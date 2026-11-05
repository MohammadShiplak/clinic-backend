using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services;

public class AuthService(
    ClinicDbContext context,
    IPasswordService passwords,
    TimeProvider clock) : IAuthService
{
    private const string EmailTakenMessage = "An account with this email already exists.";

    public async Task<ServiceResult<RegisterResultDto>> RegisterAsync(
        RegisterDto dto, CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(dto.Email);

        // Cheap early exit BEFORE the expensive hash. The unique index below is the real guarantee.
        if (await context.UserAccounts.AnyAsync(u => u.Email == email, ct))
            return ServiceResult<RegisterResultDto>.Fail(ServiceError.Conflict, EmailTakenMessage);

        var account = new UserAccount
        {
            Email = email,
            PasswordHash = passwords.Hash(dto.Password),
            Role = UserRole.Patient,                    // server decides, never the client
            IsActive = true,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime
        };

        var patient = new Patient
        {
            Person = new Person
            {
                Name = dto.FullName.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = email,
                Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
                UserAccount = account                   // navigation, NOT PersonId: EF fills the FK on save
            }
        };

        context.Patients.Add(patient);   // adds the whole graph: Patient + Person + UserAccount

        try
        {
            await context.SaveChangesAsync(ct);         // one implicit transaction: all three rows or none
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Two requests passed the AnyAsync check at the same time; the index stopped the second.
            return ServiceResult<RegisterResultDto>.Fail(ServiceError.Conflict, EmailTakenMessage);
        }

        return ServiceResult<RegisterResultDto>.Ok(
            new RegisterResultDto(patient.PatientId, patient.PersonId, email));
    }
}