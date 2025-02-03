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
        private static byte[] _defaultKey = Encoding.UTF8.GetBytes("YourStrongDefaultKeyHere1234567890123456");
        private static byte[] _key;

        internal static byte[] GenerateRandomKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[32]; // 256 bits for AES
                rng.GetBytes(key);
                return key;
            }
        }
        internal static byte[] GenerateRandomIV()
        {
             using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] iv = new byte[16]; // 128 bits for AES
                rng.GetBytes(iv);
                return iv;
            }
        }
        internal static void SetEncryptionKey(byte[] key)
        {
            _key = key;
        }
        internal static void SetEncryptionKey(string? dnsKey)
        {
            if (!string.IsNullOrEmpty(dnsKey))
            {
                try
                {
                    _key = Convert.FromBase64String(dnsKey);
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

        internal static string Encrypt(string plainText, byte[] iv)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = iv;

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

         internal static string Decrypt(string cipherText, string ivString)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] iv = Convert.FromBase64String(ivString);
                if (iv.Length != 16)
                {
                    InstallationSettings.log?.WriteLine($"Error: Invalid IV length. Expected 16 bytes, got {iv.Length} bytes.");
                    return null;
                }

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;
                    aesAlg.IV = iv;
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
                return null;
            }
        }
    }
}