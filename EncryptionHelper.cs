using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RustdeskSetup
{
    internal static class EncryptionHelper
    {
        // IMPORTANT: This key should be generated securely and kept secret.
        // DO NOT hardcode a key like this in a real application.
        private static readonly byte[] _key = Encoding.UTF8.GetBytes("GLJUqYaXKn!v4^5vdrPr3yaKD#d!*2gn"); // 32 bytes for AES-256
        private static readonly byte[] _iv = Encoding.UTF8.GetBytes("Hsn2aC@jk4HC5awc"); // 16 bytes for AES

        internal static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        internal static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}