using Microsoft.AspNetCore.Identity;

namespace Store.Services
{
    public interface IPasswordHasherService
    {
        string HashPassword(string userName, string password);
        bool VerifyHashedPassword(string userName, string hashedPassword, string providedPassword);
    }

    public class PasswordHasherService : IPasswordHasherService
    {
        private readonly PasswordHasher<string> _hasher = new();

        public string HashPassword(string userName, string password) =>
            _hasher.HashPassword(userName, password);

        public bool VerifyHashedPassword(string userName, string hashedPassword, string providedPassword)
        {
            var res = _hasher.VerifyHashedPassword(userName, hashedPassword, providedPassword);
            return res == PasswordVerificationResult.Success || res == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
