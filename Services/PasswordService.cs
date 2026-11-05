using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using Microsoft.AspNetCore.Identity;
namespace ClinicManagementSystem.API.Services
{
    public class PasswordService : IPasswordService
    {

        private readonly PasswordHasher<UserAccount> _hasher = new();

        private static readonly UserAccount Placeholder = new();

        public string Hash(string password) => _hasher.HashPassword(Placeholder, password);
       
        public bool Verify(string passwordHash, string password)
        {
           var result=_hasher.VerifyHashedPassword(Placeholder, passwordHash, password);

            return result is PasswordVerificationResult.Success
                    or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
