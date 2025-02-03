using System;
using System.IO;
using System.Text;

namespace RustdeskSetup
{
    internal static class DnsSettingsGenerator
    {
        internal static void GenerateAndSaveDnsSettings(string password, string customKey)
        {
             InstallationSettings.log?.WriteLine("Generating DNS settings...");

            byte[] key;
            if (!string.IsNullOrEmpty(customKey))
            {
                if (Encoding.UTF8.GetBytes(customKey).Length != 32)
                {
                    InstallationSettings.log?.WriteLine("Custom key must be 32 characters. Using a randomly generated key.");
                    key = EncryptionHelper.GenerateRandomKey();
                }
                else
                {
                    key = Encoding.UTF8.GetBytes(customKey);
                }
            }
            else
            {
                key = EncryptionHelper.GenerateRandomKey();
            }
            string keyString = Encoding.UTF8.GetString(key);
            string ivString = Encoding.UTF8.GetString(EncryptionHelper.GetIV());
            EncryptionHelper.SetEncryptionKey(keyString);
            string encryptedPassword = EncryptionHelper.Encrypt(password);


            string dnsSettingsFilePath = Path.Combine(Environment.CurrentDirectory, "dnssettings.txt");

             try
            {
                 using (StreamWriter writer = new StreamWriter(dnsSettingsFilePath))
                {
                    writer.WriteLine("DNS TXT Record Settings:");
                    writer.WriteLine($"_rdpw=\"{encryptedPassword}\"");
                    writer.WriteLine($"_rdkey=\"{keyString}\"");
                    writer.WriteLine($"_rdiv=\"{ivString}\"");
                }
                InstallationSettings.log?.WriteLine($"DNS settings saved to: {dnsSettingsFilePath}");
                Console.WriteLine($"DNS settings saved to: {dnsSettingsFilePath}");
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error saving DNS settings: {ex.Message}");
                 Console.WriteLine($"Error saving DNS settings: {ex.Message}");
            }
        }
    }
}