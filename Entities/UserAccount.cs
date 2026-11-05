namespace ClinicManagementSystem.API.Entities;

public enum UserRole : byte
{
    Patient = 1,
    Doctor = 2,
    Admin = 3
}

public class UserAccount
{
    public int UserAccountId { get; set; }

    public int PersonId { get; set; }

    // Login identifier. Always stored trimmed + lower-case (we normalize in the service).
    public string Email { get; set; } = null!;

    // NEVER the password. Only the salted, slow hash.
    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public Person Person { get; set; } = null!;
}