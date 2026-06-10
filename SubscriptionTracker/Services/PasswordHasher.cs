using System;
using System.Security.Cryptography;
using System.Text;

namespace SubscriptionTracker.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (password == null) return string.Empty;
            
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
