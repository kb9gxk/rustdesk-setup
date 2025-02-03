using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RustdeskSetup
{
    internal static class EncryptionHelper
    {
        // Default key to use if DNS record is not found.
        // IMPORTANT: This should be a very strong key, and it is still better to use a key from DNS.
        private static readonly byte[] _defaultKey = Encoding.UTF8.GetBytes("DefaultKey123456789012345678901234567");
        private static readonly byte[] _iv = Encoding.UTF8.GetBytes("Hsn2aC@jk4HC5awc"); // 16 bytes for AES
        private static byte[] _key;

        internal static void SetEncryptionKey(string? dnsKey)
        {
            if (!string.IsNullOrEmpty(dnsKey))
            {
                try
                {
                    _key = Encoding.UTF8.GetBytes(dnsKey);
                    if (_key.Length != 32)
                    {
                        InstallationSettings.log?.WriteLine($"Warning: DNS Key is not 32 bytes. Using default key.");
                        _key = _defaultKey;
                    }
                }
                catch (Exception ex)
                {
                    InstallationSettings.log?.WriteLine($"Error setting DNS Key: {ex.Message}. Using default key.");
                    _key = _defaultKey;
                }
            }
            else
            {
                InstallationSettings.log?.WriteLine($"Warning: DNS Key not found. Using default key.");
                _key = _defaultKey;
            }
        }

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

            try
            {
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
                        string decryptedText = srDecrypt.ReadToEnd();
                        InstallationSettings.log?.WriteLine("Password decrypted successfully.");
                        return decryptedText;
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error during password decryption: {ex.Message}");
                return string.Empty; // Or handle the error as needed
            }
        }
    }
}