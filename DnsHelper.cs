using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
                InstallationSettings.log?.WriteLine("Starting DNS TXT record lookup for kb9gxk.net using nslookup...");
                var txtRecords = await LookupTxtRecordsWithNsLookupAsync("kb9gxk.net");
                InstallationSettings.log?.WriteLine("Finished DNS TXT record lookup using nslookup.");

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
                            if (rustdeskPw == null)
                            {
                                InstallationSettings.log?.WriteLine($"Warning: Password decryption failed.");
                            }
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

        private static async Task<List<string>> LookupTxtRecordsWithNsLookupAsync(string domain)
        {
            List<string> txtRecords = new List<string>();
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "nslookup";
                    process.StartInfo.Arguments = $"-type=txt {domain}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        InstallationSettings.log?.WriteLine($"nslookup exited with code {process.ExitCode}. Error: {error}");
                        return txtRecords; // Return empty list on error
                    }
                    // Regex to extract TXT record content
                    string pattern = @"""([^""]*)""";
                    MatchCollection matches = Regex.Matches(output, pattern);
                    foreach (Match match in matches)
                    {
                        txtRecords.Add(match.Groups[1].Value);
                    }

                    if (txtRecords.Count == 0)
                    {
                        InstallationSettings.log?.WriteLine($"❌ No TXT records found for {domain} using nslookup.");
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"❌ Error during nslookup: {ex.Message}");
            }
            return txtRecords;
        }
    }
}