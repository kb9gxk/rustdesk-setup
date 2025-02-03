using System;
using System.IO;
using System.Text;

namespace RustdeskSetup
{
    internal static class DnsSettingsGenerator
    {
        internal static void GenerateAndSaveDnsSettings(string password)
        {
             InstallationSettings.log?.WriteLine("Generating DNS settings...");

            byte[] key = EncryptionHelper.GenerateRandomKey();
            byte[] iv = EncryptionHelper.GenerateRandomIV();
            string keyString = Convert.ToBase64String(key); // Base64 encode the key
            string ivString = Convert.ToBase64String(iv);  // Base64 encode the IV
            EncryptionHelper.SetEncryptionKey(key);
            string encryptedPassword = EncryptionHelper.Encrypt(password, iv);


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