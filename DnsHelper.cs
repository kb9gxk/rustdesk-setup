using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;

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
                using var client = new LookupClient();
                var result = await client.QueryAsync(domain, QueryType.TXT);

                if (result.HasError)
                {
                    InstallationSettings.log?.WriteLine($"DNS query error: {result.ErrorMessage}");
                    return txtRecords;
                }

                foreach (var record in result.Answers.TxtRecords())
                {
                     foreach (var txt in record.Text)
                    {
                        txtRecords.Add(txt);
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