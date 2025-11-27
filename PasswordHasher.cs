using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp
{
    internal class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string ToPBKDF2(string password)
        {
            // generate salt
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // derive key
            byte[] derivedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(derivedKey)}";
        }
        
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 3);
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedKey = Convert.FromBase64String(parts[2]);


            byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expectedKey.Length);

            // constant-time comparison
            return CryptographicOperations.FixedTimeEquals(expectedKey, actualKey);
        }
    }
}
