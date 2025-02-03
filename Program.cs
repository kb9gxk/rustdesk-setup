using System;
using System.IO;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            InstallationSettings.RedirectConsoleOutput();
            InstallationSettings.HideWindow();

            CommandLineArgs parsedArgs = CommandLineArgs.Parse();

            if (parsedArgs.ShouldShowHelp)
            {
                CommandLineArgs.ShowHelp();
                InstallationSettings.ResetConsoleOutput();
                return;
            }

            Configuration.UseStableVersion = parsedArgs.UseStableVersion;

            string? dnsConfig = null;
            string? dnsPassword = null;
            string? dnsKey = null;

            if (parsedArgs.IsJeffBuild)
            {
                InstallationSettings.log?.WriteLine("Jeff Build Detected, checking DNS TXT records");
                Configuration.SetJeffDefaults();
                InstallationSettings.log?.WriteLine("Attempting to retrieve DNS TXT records...");
                try
                {
                    (dnsConfig, dnsPassword, dnsKey) = await DnsHelper.GetRustdeskConfigFromDnsAsync();
                    InstallationSettings.log?.WriteLine("Finished retrieving DNS TXT records.");
                    if (!string.IsNullOrEmpty(dnsConfig))
                    {
                        Configuration.RustdeskCfg = dnsConfig;
                        InstallationSettings.log?.WriteLine($"DNS Config found: {dnsConfig}");
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine("DNS Config not found.");
                    }
                    if (!string.IsNullOrEmpty(dnsPassword))
                    {
                        Configuration.RustdeskPw = dnsPassword;
                        InstallationSettings.log?.WriteLine("DNS Password found and decrypted.");
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine("DNS Password not found.");
                    }
                    if (!string.IsNullOrEmpty(dnsKey))
                    {
                        InstallationSettings.log?.WriteLine($"DNS Encryption Key found.");
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine("DNS Encryption Key not found.");
                    }
                }
                catch (Exception ex)
                {
                    InstallationSettings.log?.WriteLine($"Error during DNS lookup: {ex.Message}");
                }

            }
            else
            {
                Configuration.RustdeskCfg = parsedArgs.RustdeskCfg;
                Configuration.RustdeskPw = parsedArgs.RustdeskPw;
            }

            EncryptionHelper.SetEncryptionKey(dnsKey); // Set the encryption key from DNS or default

            string apiUrl = Configuration.UseStableVersion.Value ? InstallationSettings.githubStableApiUrl : InstallationSettings.githubNightlyApiUrl;

            (string? downloadUrl, string? version) = await GitHubHelper.GetLatestRustdeskInfoAsync(apiUrl);

            if (downloadUrl == null || version == null)
            {
                InstallationSettings.log?.WriteLine("Failed to get Rustdesk download information.");
                InstallationSettings.ResetConsoleOutput();
                return;
            }

            await Installation.DownloadAndInstallRustdeskAsync(downloadUrl, InstallationSettings.tempDir);

            string rustdeskDir = Installation.GetRustdeskDirectory();
            string runMe = Installation.GetRustdeskExecutable(rustdeskDir);
            string? rustdeskId = Installation.GetRustdeskId(runMe, rustdeskDir);

            if (rustdeskId != null)
            {
                Utility.ConfigureAndRunRustdesk(rustdeskId, runMe, Configuration.RustdeskCfg, Configuration.RustdeskPw);
                Utility.SaveRustdeskInfo(rustdeskId);
                Utility.DisplayPopup(rustdeskId, version);
            }
            else
            {
                InstallationSettings.log?.WriteLine("Rustdesk ID not found.");
            }

            if (!string.IsNullOrEmpty(InstallationSettings.rustdeskExe))
            {
                Installation.Cleanup(InstallationSettings.tempDir, InstallationSettings.rustdeskExe);
            }
            else
            {
                InstallationSettings.log?.WriteLine("Skipping cleanup, rustdeskExe is null or empty.");
            }
            InstallationSettings.ResetConsoleOutput();
        }
    }
}