using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace RevenueBillingApp.Extensions
{
    public class DataEncryptor
    {
        private readonly IConfiguration _config;
        private static int SaltSize;
        private static int KeySize;
        private static int Iterations;
        private static string EncryptionKey_;
        public DataEncryptor(IConfiguration config)
        {
            _config = config;
            SaltSize = Convert.ToInt32(_config["AM_SALT_SIZE"]);
            KeySize = Convert.ToInt32(_config["AM_KEY_SIZE"]);
            Iterations = Convert.ToInt32(_config["AM_ITERATIONS"]);
            EncryptionKey_ = _config["AM_ENCRYPTION_KEY"];
        }

        public string EncryptPassword(string clearText)
        {
            try
            {
                string EncryptionKey = EncryptionKey_;
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {

            }
            return clearText;
        }

        public string DecryptPassword(string cipherText)
        {
            try
            {
                string EncryptionKey = EncryptionKey_;
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {

            }
            return cipherText;
        }

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using PBKDF2
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Combine the salt and password hash
            byte[] hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            // Convert to base64 string for storage
            string base64Hash = Convert.ToBase64String(hashBytes);
            return base64Hash;
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Convert the hashed password from base64 string
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt from the stored hash
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Compute the hash of the provided password
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            // Compare the computed hash with the stored hash
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
