using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Identity;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<UserAccount> _hasher = new();
    private static readonly UserAccount Placeholder = new();

    // Hashed ONCE at startup (the service is a singleton). Used only to burn the same CPU time.
    private readonly string _dummyHash;

    public PasswordService() => _dummyHash = _hasher.HashPassword(Placeholder, "not-a-real-password");

    public string Hash(string password) => _hasher.HashPassword(Placeholder, password);

    public bool Verify(string? passwordHash, string password)
    {
        if (passwordHash is null)
        {
            // Unknown email: do the same expensive work so response time doesn't reveal it.
            _hasher.VerifyHashedPassword(Placeholder, _dummyHash, password);
            return false;
        }

        var result = _hasher.VerifyHashedPassword(Placeholder, passwordHash, password);
        return result is PasswordVerificationResult.Success
                      or PasswordVerificationResult.SuccessRehashNeeded;
    }
}