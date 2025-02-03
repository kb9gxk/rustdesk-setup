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
            // InstallationSettings.HideWindow();  // Removed for testing

            CommandLineArgs parsedArgs = CommandLineArgs.Parse();

            if (parsedArgs.ShouldShowHelp)
            {
                CommandLineArgs.ShowHelp();
                InstallationSettings.ResetConsoleOutput();
                return;
            }
            if (parsedArgs.GenerateDnsRecords)
            {
                DnsSettingsGenerator.GenerateAndSaveDnsSettings(parsedArgs.RustdeskPw);
                InstallationSettings.ResetConsoleOutput();
                return;
            }

            string? dnsConfig = null;
            string? dnsPassword = null;
            string? dnsKey = null;
            string? dnsIv = null;

            if (parsedArgs.ShouldTest) // New test logic
            {
                if (!parsedArgs.IsJeffBuild)
                {
                    Console.WriteLine("The --test argument is only valid for Jeff builds.");
                    InstallationSettings.ResetConsoleOutput();
                    return;
                }
                Console.WriteLine("Jeff Build Detected, testing DNS TXT records and decryption...");
                Configuration.SetJeffDefaults();
                
                try
                {
                     (dnsConfig, dnsPassword, dnsKey, dnsIv) = await DnsHelper.GetRustdeskConfigFromDnsAsync();
                    Console.WriteLine($"DNS Config: {dnsConfig ?? "Not Found"}");
                    Console.WriteLine($"DNS Password: {dnsPassword ?? "Not Found"}");
                    Console.WriteLine($"DNS Key: {dnsKey ?? "Not Found"}");
                    Console.WriteLine($"DNS IV: {dnsIv ?? "Not Found"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during DNS lookup: {ex.Message}");
                }
                InstallationSettings.ResetConsoleOutput();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }


            Configuration.UseStableVersion = parsedArgs.UseStableVersion;


            if (parsedArgs.IsJeffBuild)
            {
                InstallationSettings.log?.WriteLine("Jeff Build Detected, checking DNS TXT records");
                Configuration.SetJeffDefaults();
                InstallationSettings.log?.WriteLine("Attempting to retrieve DNS TXT records...");
                try
                {
                    (dnsConfig, dnsPassword, dnsKey, dnsIv) = await DnsHelper.GetRustdeskConfigFromDnsAsync();
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
                        EncryptionHelper.SetEncryptionKey(dnsKey);
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine("DNS Encryption Key not found. Using default key.");
                        EncryptionHelper.SetEncryptionKey((string)null); // Set to null to trigger default key logic
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