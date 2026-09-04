using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;

namespace RevenueBillingApi.Extensions
{
    public class CommonMethods
    {
        private readonly IConfiguration _config;
        private static int SaltSize;
        private static int KeySize;
        private static int Iterations;
        private static string EncryptionKey_;

        public CommonMethods(IConfiguration config)
        {
            _config = config;


            SaltSize = Convert.ToInt32(Environment.GetEnvironmentVariable("SALT_SIZE"));
            KeySize = Convert.ToInt32(Environment.GetEnvironmentVariable("KEY_SIZE"));
            Iterations = Convert.ToInt32(Environment.GetEnvironmentVariable("ITERATIONS"));
         
            EncryptionKey_ = Environment.GetEnvironmentVariable("xpt956wxp");

        }
        //public string EncryptPassword(string clearText)
        //{
        //    try
        //    {
        //        string EncryptionKey = EncryptionKey_;
        //        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        //        using (Aes encryptor = Aes.Create())
        //        {
        //            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //            encryptor.Key = pdb.GetBytes(32);
        //            encryptor.IV = pdb.GetBytes(16);
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        //                {
        //                    cs.Write(clearBytes, 0, clearBytes.Length);
        //                    cs.Close();
        //                }
        //                clearText = Convert.ToBase64String(ms.ToArray());
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    return clearText;
        //}

        //public string DecryptPassword(string cipherText)
        //{
        //    try
        //    {
        //        string EncryptionKey = EncryptionKey_;
        //        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        //        using (Aes encryptor = Aes.Create())
        //        {
        //            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //            encryptor.Key = pdb.GetBytes(32);
        //            encryptor.IV = pdb.GetBytes(16);
        //            using (MemoryStream ms = new MemoryStream())
        //            {
        //                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
        //                {
        //                    cs.Write(cipherBytes, 0, cipherBytes.Length);
        //                    cs.Close();
        //                }
        //                cipherText = Encoding.Unicode.GetString(ms.ToArray());
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    return cipherText;
        //}

        public string EncryptPassword(string clearText)
        {
            string key = EncryptionKey_;
            byte[] keyBytes = GenerateKey(key);
            byte[] iv = GenerateIV(12);
            byte[] plainBytes = Encoding.UTF8.GetBytes(clearText);

            using (AesGcm aesGcm = new AesGcm(keyBytes))
            {
                byte[] cipherBytes = new byte[plainBytes.Length];
                byte[] tag = new byte[16];

                aesGcm.Encrypt(iv, plainBytes, cipherBytes, tag);
                return $"{Convert.ToBase64String(iv)}.{Convert.ToBase64String(cipherBytes)}.{Convert.ToBase64String(tag)}";
            }
        }

        //public string DecryptPassword(string cipherText)
        //{
        //    if (string.IsNullOrWhiteSpace(cipherText))
        //    {
        //        return string.Empty;
        //    }
        //    string key = EncryptionKey_;
        //    byte[] keyBytes = GenerateKey(key);
        //    string[] parts = cipherText.Split('.');
        //    if (parts.Length != 3)
        //        throw new ArgumentException("Invalid cipher text format");

        //    byte[] iv = Convert.FromBase64String(parts[0]);
        //    byte[] cipherBytes = Convert.FromBase64String(parts[1]);
        //    byte[] tag = Convert.FromBase64String(parts[2]);
        //    using (AesGcm aesGcm = new AesGcm(keyBytes))
        //    {
        //        byte[] plainBytes = new byte[cipherBytes.Length];
        //        aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);
        //        return Encoding.UTF8.GetString(plainBytes);
        //    }
        //}

        public string DecryptPassword(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return string.Empty;
            }

            string key = EncryptionKey_;
            byte[] keyBytes = GenerateKey(key);

            string[] parts = cipherText.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid cipher text format");

            try
            {
                byte[] iv = Convert.FromBase64String(parts[0]);
                byte[] cipherBytes = Convert.FromBase64String(parts[1]);
                byte[] tag = Convert.FromBase64String(parts[2]);

                using (AesGcm aesGcm = new AesGcm(keyBytes))
                {
                    byte[] plainBytes = new byte[cipherBytes.Length];
                    aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid Base64 string in cipher text parts", ex);
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException("Decryption failed. Please check the encryption key or cipher text.", ex);
            }
        }
        private byte[] GenerateKey(string plainKey)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(plainKey)).Take(32).ToArray();
            }
        }
        private byte[] GenerateIV(int size)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] iv = new byte[size];
                rng.GetBytes(iv);
                return iv;
            }
        }

        //public string HashPassword(string password)
        //{
        //    byte[] salt = new byte[SaltSize];
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(salt);
        //    }

        //    byte[] hash = KeyDerivation.Pbkdf2(
        //        password: password,
        //        salt: salt,
        //        prf: KeyDerivationPrf.HMACSHA256,
        //        iterationCount: Iterations,
        //        numBytesRequested: KeySize);

        //    byte[] hashBytes = new byte[SaltSize + KeySize];
        //    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        //    Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        //    string base64Hash = Convert.ToBase64String(hashBytes);
        //    return base64Hash;
        //}
        //public bool VerifyPassword(string password, string hashedPassword)
        //{
        //    byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        //    byte[] salt = new byte[SaltSize];
        //    Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        //    byte[] hash = KeyDerivation.Pbkdf2(
        //        password: password,
        //        salt: salt,
        //        prf: KeyDerivationPrf.HMACSHA256,
        //        iterationCount: Iterations,
        //        numBytesRequested: KeySize);

        //    for (int i = 0; i < KeySize; i++)
        //    {
        //        if (hashBytes[i + SaltSize] != hash[i])
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}



        public string HashPassword(string password)
        {
            byte[] salt = GenerateSalt(16);
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] hash = argon2.GetBytes(32);
                return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

            }
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split('.');



            if (parts.Length != 2) return false;

            byte[] saltt = Convert.FromBase64String(parts[0]);
            byte[] hashToCompare = Convert.FromBase64String(parts[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = saltt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] computedHash = argon2.GetBytes(32);
                return computedHash.SequenceEqual(hashToCompare);
            }
        }
        private byte[] GenerateSalt(int size)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }

    }
}