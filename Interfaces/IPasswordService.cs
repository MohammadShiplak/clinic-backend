namespace ClinicManagementSystem.API.Interfaces
{
    public interface IPasswordService
    {
        string Hash(string password);

        /// True when the password matches the stored hash.
        bool Verify(string passwordHash, string password);


    }
}
