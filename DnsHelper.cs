using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                        rustdeskPw = EncryptionHelper.Decrypt(encryptedPw);
                    }
                    else if (trimmedRecord.StartsWith(KeyRecordName + "="))
                    {
                        encryptionKey = trimmedRecord.Substring(KeyRecordName.Length + 1).Trim();
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
            return await Task.Run(() =>
            {
                List<string> txtRecords = new();
                try
                {
                    string command = "nslookup";
                    string arguments = $"-query=TXT {domain}";


                    InstallationSettings.log?.WriteLine($"🔍 Executing DNS lookup: {command} {arguments}");
                    string output = RunCommand(command, arguments);

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        txtRecords.AddRange(ParseTxtRecords(output));
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine($"❌ No TXT records found for {domain}");
                    }
                }
                catch (Exception ex)
                {
                    InstallationSettings.log?.WriteLine($"❌ DNS TXT lookup failed: {ex.Message}");
                }

                return txtRecords;
            });
        }

        private static string RunCommand(string command, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new() { StartInfo = startInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    InstallationSettings.log?.WriteLine($"⚠️ DNS lookup error: {error}");
                }

                return output;
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"❌ Failed to run command: {ex.Message}");
                return string.Empty;
            }
        }

        private static List<string> ParseTxtRecords(string rawOutput)
        {
            List<string> txtRecords = new();

            foreach (string line in rawOutput.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                {
                    txtRecords.Add(trimmed.Trim('"'));
                }
            }

            return txtRecords;
        }
    }
}