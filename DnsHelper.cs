using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class DnsHelper
    {
        private const string ConfigRecordName = "_rdcfg";
        private const string PasswordRecordName = "_rdpw";
        private const string KeyRecordName = "_rdkey";
        private const string IVRecordName = "_rdiv";

        internal static async Task<(string? rustdeskCfg, string? rustdeskPw, string? encryptionKey, string? encryptionIV)> GetRustdeskConfigFromDnsAsync()
        {
            string? rustdeskCfg = null;
            string? rustdeskPw = null;
            string? encryptionKey = null;
            string? encryptionIV = null;

            try
            {
                InstallationSettings.log?.WriteLine("Starting DNS TXT record lookup for kb9gxk.net...");
                var txtRecords = await LookupTxtRecordsAsync("kb9gxk.net");
                InstallationSettings.log?.WriteLine("Finished DNS TXT record lookup.");

                foreach (var record in txtRecords)
                {
                    string trimmedRecord = record.Trim().TrimStart('\uFEFF'); // Trim BOM and whitespace
                    InstallationSettings.log?.WriteLine($"Found DNS TXT record: {trimmedRecord}");

                    if (trimmedRecord.StartsWith(ConfigRecordName + "="))
                    {
                        rustdeskCfg = trimmedRecord.Substring(ConfigRecordName.Length + 1).Trim();
                    }
                    else if (trimmedRecord.StartsWith(PasswordRecordName + "="))
                    {
                        string encryptedPw = trimmedRecord.Substring(PasswordRecordName.Length + 1).Trim();
                         if (encryptedPw.StartsWith("="))
                        {
                            encryptedPw = encryptedPw.Substring(1);
                        }
                        if (!string.IsNullOrEmpty(encryptionIV))
                        {
                            rustdeskPw = EncryptionHelper.Decrypt(encryptedPw, encryptionIV);
                        }
                        else
                        {
                            InstallationSettings.log?.WriteLine("Warning: IV not found in DNS. Cannot decrypt password.");
                        }
                    }
                    else if (trimmedRecord.StartsWith(KeyRecordName + "="))
                    {
                        encryptionKey = trimmedRecord.Substring(KeyRecordName.Length + 1).Trim();
                    }
                    else if (trimmedRecord.StartsWith(IVRecordName + "="))
                    {
                        encryptionIV = trimmedRecord.Substring(IVRecordName.Length + 1).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching DNS TXT records: {ex.Message}");
            }

            return (rustdeskCfg, rustdeskPw, encryptionKey, encryptionIV);
        }

        private static async Task<List<string>> LookupTxtRecordsAsync(string domain)
        {
            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(domain);
                List<string> txtRecords = new();
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                         InstallationSettings.log?.WriteLine($"🔍 Performing DNS TXT lookup for {domain} at IP {address}...");
                         IPHostEntry dnsEntry = await Dns.GetHostEntryAsync(domain);
                         foreach (var dnsAddress in dnsEntry.AddressList)
                         {
                             if (dnsAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                             {
                                 try
                                 {
                                     var txtRecord = await Dns.GetHostEntryAsync(domain);
                                     txtRecords.AddRange(txtRecord.Aliases);
                                 }
                                 catch (Exception ex)
                                 {
                                     InstallationSettings.log?.WriteLine($"❌ DNS TXT lookup failed for {domain} at {dnsAddress}: {ex.Message}");
                                 }
                             }
                         }
                    }
                }
                if (txtRecords.Count == 0)
                {
                    InstallationSettings.log?.WriteLine($"❌ No TXT records found for {domain}");
                }
                return txtRecords;
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"❌ DNS TXT lookup failed: {ex.Message}");
                return new List<string>();
            }
        }
    }
}