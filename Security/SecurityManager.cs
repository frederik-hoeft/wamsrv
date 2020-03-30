using Scrypt;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace wamsrv.Security
{
    public static class SecurityManager
    {
        // TODO: Catch encrypt / decrypt errors
        private readonly static RNGCryptoServiceProvider rngCryptoService = new RNGCryptoServiceProvider();
        #region AES
        // using AES with:
        // Key hash algorithm: SHA-256
        // Key Size: 256 Bit
        // Block Size: 128 Bit
        // Input Vector (IV): 128 Bit
        // Mode of Operation: Cipher-Block Chaining (CBC)
        /// <summary>
        /// Encrypts a plain text with a password using AES-256 CBC with SHA-256.
        /// </summary>
        /// <param name="plainText">The text to be encrypted.</param>
        /// <param name="password">The password to encrypt the text with.</param>
        /// <returns>The encrypted text.</returns>
        public static string AESEncrypt(string plainText, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashedPasswordBytes = SHA256Managed.Create().ComputeHash(passwordBytes);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using AesCng AES = new AesCng
                {
                    KeySize = 256,
                    Key = hashedPasswordBytes
                };
                AES.IV = GetRandomBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;

                using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cs.Close();
                }
                encryptedBytes = new byte[AES.IV.Length + ms.ToArray().Length];
                AES.IV.CopyTo(encryptedBytes, 0);
                ms.ToArray().CopyTo(encryptedBytes, AES.IV.Length);
            }
            return Convert.ToBase64String(encryptedBytes);
        }
        /// <summary>
        /// Decrypts a cipher text with a password using AES-256 CBC with SHA-256.
        /// </summary>
        /// <param name="cipherText">The cipher text to be decrypted.</param>
        /// <param name="password">The password to decrypt the cipher text with.</param>
        /// <returns>The decrypted text.</returns>
        public static string AESDecrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }
            byte[] iv = new byte[16];
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] hashedPasswordBytes = SHA256Managed.Create().ComputeHash(passwordBytes);
            Array.Copy(cipherBytes, iv, 16);
            byte[] decryptedBytes = null;
            using (AesCng AES = new AesCng())
            {
                AES.IV = iv;
                AES.KeySize = 256;
                AES.Mode = CipherMode.CBC;
                AES.Key = hashedPasswordBytes;
                using ICryptoTransform decryptor = AES.CreateDecryptor();
                using MemoryStream msDecrypted = new MemoryStream();
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypted, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(cipherBytes, 16, cipherBytes.Length - 16);
                    csDecrypt.Close();
                }
                decryptedBytes = msDecrypted.ToArray();
            }
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        /// <summary>
        /// Generates cryptographically secure random bytes.
        /// </summary>
        /// <param name="saltLength">The number of bytes to be generated.</param>
        /// <returns>Cryptographically secure random bytes.</returns>
        public static byte[] GetRandomBytes(int saltLength)
        {
            byte[] randomBytes = new byte[saltLength];
            RandomNumberGenerator.Create().GetBytes(randomBytes);
            return randomBytes;
        }
        #endregion
        #region SCrypt
        /// <summary>
        /// Creates the scrypt hash of a password and salt.
        /// </summary>
        /// <param name="password">The password to be hashed.</param>
        /// <returns>The hex encoded salted scrypt hash of the password.</returns>
        public static string ScryptHash(string password)
        {
            ScryptEncoder scrypt = new ScryptEncoder(65536, 8, 1, rngCryptoService);
            return scrypt.Encode(password);
        }

        public static bool ScryptCheck(string password, string hash)
        {
            ScryptEncoder scrypt = new ScryptEncoder(65536, 8, 1, rngCryptoService);
            return scrypt.Compare(password, hash);
        }
        #endregion
        public static string GenerateSecurityToken()
        {
            using SHA512Managed hashFunction = new SHA512Managed();
            byte[] plainBytes = GetRandomBytes(2048);
            byte[] token = hashFunction.ComputeHash(plainBytes);
            return Convert.ToBase64String(token);
        }

        public unsafe static string GenerateSecurityCode()
        {
            int length = MainServer.Config.WamsrvSecurityConfig.TwoFactorCodeLength * sizeof(uint);
            byte[] buffer = new byte[length];
            rngCryptoService.GetBytes(buffer);
            uint[] ints = new uint[length];
            fixed (byte* b = buffer)
            {
                for (int i = 0; i < length; i++)
                {
                    ints[i] = *(uint*)(b + i * sizeof(uint));
                }
            }
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                uint result = ints[i] % 10;
                builder.Append(result.ToString());
            }
            return builder.ToString();
        }

        public static string GenerateHid()
        {
            using SHA256Managed hashFunction = new SHA256Managed();
            byte[] plainBytes = GetRandomBytes(2048);
            byte[] passwordBytes = hashFunction.ComputeHash(plainBytes);
            return Convert.ToBase64String(passwordBytes);
        }

        public static string DeriveUserSecret(string userid)
        {
            using SHA256Managed hashFunction = new SHA256Managed();
            byte[] plainBytes = Encoding.UTF8.GetBytes(userid + MainServer.Config.WamsrvSecurityConfig.ServerSecret);
            byte[] passwordBytes = hashFunction.ComputeHash(plainBytes);
            return Convert.ToBase64String(passwordBytes);
        }
    }
}
