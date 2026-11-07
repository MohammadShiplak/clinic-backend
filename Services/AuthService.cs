using ClinicManagementSystem.API.Common;
using ClinicManagementSystem.API.DTOs;
using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.API.Services;

public class AuthService(
    ClinicDbContext context,
    IPasswordService passwords,
    IJwtTokenService tokens,
    TimeProvider clock) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(dto.Email);

        // Tracked on purpose: we update LastLoginAtUtc below.
        var account = await context.UserAccounts
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // account is null -> Verify burns the same time with a dummy hash and returns false.
        var passwordOk = passwords.Verify(account?.PasswordHash, dto.Password);

        // ONE message for: unknown email, wrong password, disabled account.
        if (account is null || !passwordOk || !account.IsActive)
            return ServiceResult<LoginResultDto>.Fail(ServiceError.Unauthorized, InvalidCredentialsMessage);

        account.LastLoginAtUtc = clock.GetUtcNow().UtcDateTime;
        await context.SaveChangesAsync(ct);

        int? patientId = null;
        if (account.Role == UserRole.Patient)
        {
            patientId = await context.Patients
                .Where(p => p.PersonId == account.PersonId)
                .Select(p => (int?)p.PatientId)
                .FirstOrDefaultAsync(ct);
        }

        var token = tokens.Create(account, patientId);

        return ServiceResult<LoginResultDto>.Ok(new LoginResultDto(
            token.Value,
            token.ExpiresAtUtc,
            new UserSummaryDto(account.PersonId, patientId, account.Person.Name, account.Email, account.Role)));
    }

    public async Task<ServiceResult<RegisterResultDto>> RegisterAsync(
        RegisterDto dto, CancellationToken ct)
    {
        var email = EmailNormalizer.Normalize(dto.Email);

        // Cheap early exit BEFORE the expensive hash. The unique index below is the real guarantee.
        if (await context.UserAccounts.AnyAsync(u => u.Email == email, ct))
            return ServiceResult<RegisterResultDto>.Fail(ServiceError.Conflict, InvalidCredentialsMessage);

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
            return ServiceResult<RegisterResultDto>.Fail(ServiceError.Conflict, "InvalidCredentialsMessage");
        }

        return ServiceResult<RegisterResultDto>.Ok(
            new RegisterResultDto(patient.PatientId, patient.PersonId, email));
    }
}