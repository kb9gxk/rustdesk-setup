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
            byte[] key = new byte[32]; // 256 bits for AES
            RandomNumberGenerator.Fill(key);
            return key;
        }
        internal static byte[] GenerateRandomIV()
        {
            byte[] iv = new byte[16]; // 128 bits for AES
            RandomNumberGenerator.Fill(iv);
            return iv;
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
                    _key = Convert.FromBase64String(dnsKey); // Convert from base64
                    if (_key.Length != 32)
                    {
                        InstallationSettings.WriteToConsoleAndLog($"Warning: DNS Key is not 32 bytes. Using default key.");
                        _key = _defaultKey;
                    }
                }
                catch (Exception ex)
                {
                    InstallationSettings.WriteToConsoleAndLog($"Error setting DNS Key: {ex.Message}. Using default key.");
                    _key = _defaultKey;
                }
            }
            else
            {
                InstallationSettings.WriteToConsoleAndLog($"Warning: DNS Key not found. Using default key.");
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

        internal static string Decrypt(string cipherText, string ivString, string? keyString)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] iv = Convert.FromBase64String(ivString); // Convert from base64
                if (iv.Length != 16)
                {
                    InstallationSettings.WriteToConsoleAndLog($"Error: Invalid IV length. Expected 16 bytes, got {iv.Length} bytes.");
                    return null;
                }

                // Get the key from the string if it exists, otherwise use the default key
                byte[] key = _defaultKey;
                if (!string.IsNullOrEmpty(keyString))
                {
                    try
                    {
                        key = Convert.FromBase64String(keyString);
                        if (key.Length != 32)
                        {
                            InstallationSettings.WriteToConsoleAndLog($"Warning: DNS Key is not 32 bytes. Using default key.");
                            key = _defaultKey;
                        }
                    }
                    catch (Exception ex)
                    {
                        InstallationSettings.WriteToConsoleAndLog($"Error setting DNS Key: {ex.Message}. Using default key.");
                        key = _defaultKey;
                    }
                }

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key; // Use the key passed in
                    aesAlg.IV = iv;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        string decryptedText = srDecrypt.ReadToEnd();
                        InstallationSettings.WriteToConsoleAndLog("Password decrypted successfully.");
                        return decryptedText;
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error during password decryption: {ex.Message}");
                return null;
            }
        }
    }
}