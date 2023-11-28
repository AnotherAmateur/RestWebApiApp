using System.Security.Cryptography;
using System.Text;

namespace RestWebApiApp.Services
{
    public static class PasswordHashService
    {
        public static string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password).Concat(salt).ToArray());
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static byte[] GenerateSalt()
        {
            var salt = RandomNumberGenerator.GetBytes(128 / 8);
            return salt;
        }  
    }
}
