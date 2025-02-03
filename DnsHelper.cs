using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class DnsHelper
    {
        private const string ConfigRecordName = "_rdcfg";
        private const string PasswordRecordName = "_rdpw";
        private const string KeyRecordName = "_rdkey";

        internal static async Task<(string? rustdeskCfg, string? rustdeskPw, string? encryptionKey)> GetRustdeskConfigFromDnsAsync()
        {
            string? rustdeskCfg = null;
            string? rustdeskPw = null;
            string? encryptionKey = null;

            try
            {
                InstallationSettings.log?.WriteLine("Starting DNS TXT record lookup for kb9gxk.net...");
                var txtRecords = await LookupTxtRecordsAsync("kb9gxk.net");
                InstallationSettings.log?.WriteLine("Finished DNS TXT record lookup.");

                foreach (var record in txtRecords)
                {
                    string trimmedRecord = record.Trim();
                    InstallationSettings.log?.WriteLine($"Found DNS TXT record: {trimmedRecord}");

                    if (trimmedRecord.StartsWith(ConfigRecordName))
                    {
                        rustdeskCfg = trimmedRecord.Substring(ConfigRecordName.Length).TrimStart('=');
                    }
                    else if (trimmedRecord.StartsWith(PasswordRecordName))
                    {
                        string encryptedPw = trimmedRecord.Substring(PasswordRecordName.Length).TrimStart('=');
                        rustdeskPw = EncryptionHelper.Decrypt(encryptedPw);
                    }
                    else if (trimmedRecord.StartsWith(KeyRecordName))
                    {
                        encryptionKey = trimmedRecord.Substring(KeyRecordName.Length).TrimStart('=');
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching DNS TXT records: {ex.Message}");
            }

            return (rustdeskCfg, rustdeskPw, encryptionKey);
        }

        private static async Task<List<string>> LookupTxtRecordsAsync(string domain)
        {
            List<string> txtRecords = new List<string>();
            try
            {
                InstallationSettings.log?.WriteLine($"Starting DNS resolution for domain: {domain}");
                var hostEntry = await Dns.GetHostEntryAsync(domain);
                InstallationSettings.log?.WriteLine($"DNS resolution completed. Addresses found: {hostEntry.AddressList.Length}");

                foreach (var address in hostEntry.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                       
                        var txtRecord = await Dns.GetHostEntryAsync(domain, address.AddressFamily);
                        if (txtRecord.Aliases.Length > 0)
                        {
                             txtRecords.AddRange(txtRecord.Aliases);
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                InstallationSettings.log?.WriteLine($"Socket Exception while resolving DNS TXT records: {ex.Message}");
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Exception while resolving DNS TXT records: {ex.Message}");
            }

            return txtRecords;
        }
    }
}