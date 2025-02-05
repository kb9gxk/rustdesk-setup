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
                    Console.WriteLine($"DNS Config: {(dnsConfig != null ? dnsConfig : "Not Found")}");
                    Console.WriteLine($"DNS Password: {(dnsPassword != null ? dnsPassword : "Not Found")}");
                     Console.WriteLine($"DNS Key: {(dnsKey != null ? dnsKey : "Not Found")}");
                    Console.WriteLine($"DNS IV: {(dnsIv != null ? dnsIv : "Not Found")}");
                    if (!string.IsNullOrEmpty(dnsPassword) && !string.IsNullOrEmpty(dnsKey) && !string.IsNullOrEmpty(dnsIv))
                    {
                        Console.WriteLine("Password Decryption: Skipped, using decrypted password from DNS.");
                    }
                    else
                    {
                        Console.WriteLine("Password Decryption: Skipped due to missing DNS values.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during DNS lookup: {ex.Message}");
                }
                InstallationSettings.ResetConsoleOutput();
                return;
            }


            Configuration.UseStableVersion = parsedArgs.UseStableVersion;


            if (parsedArgs.IsJeffBuild)
            {
                Configuration.SetJeffDefaults();
                try
                {
                    (dnsConfig, dnsPassword, dnsKey, dnsIv) = await DnsHelper.GetRustdeskConfigFromDnsAsync();
                    if (!string.IsNullOrEmpty(dnsConfig))
                    {
                        Configuration.RustdeskCfg = dnsConfig;
                    }
                    if (!string.IsNullOrEmpty(dnsPassword))
                    {
                        Configuration.RustdeskPw = dnsPassword;
                    }
                    if (!string.IsNullOrEmpty(dnsKey))
                    {
                        EncryptionHelper.SetEncryptionKey(dnsKey);
                    }
                    else
                    {
                        EncryptionHelper.SetEncryptionKey((string)null); // Set to null to trigger default key logic
                    }
                    if (!string.IsNullOrEmpty(dnsPassword) && !string.IsNullOrEmpty(dnsKey) && !string.IsNullOrEmpty(dnsIv))
                    {
                        InstallationSettings.WriteToConsoleAndLog("Decrypting Password: Skipped, using decrypted password from DNS.");
                    }
                    else
                    {
                        InstallationSettings.WriteToConsoleAndLog("Decrypting Password: Skipped due to missing DNS values.");
                    }
                }
                catch (Exception ex)
                {
                    InstallationSettings.WriteToConsoleAndLog($"Error during DNS lookup: {ex.Message}");
                }

            }
            else
            {
                Configuration.RustdeskCfg = parsedArgs.RustdeskCfg;
                Configuration.RustdeskPw = parsedArgs.RustdeskPw;
            }

             // Set the encryption key from DNS or default (this is done in the Jeff build logic)
           

            string apiUrl = Configuration.UseStableVersion.Value ? InstallationSettings.githubStableApiUrl : InstallationSettings.githubNightlyApiUrl;

            (string? downloadUrl, string? version) = await GitHubHelper.GetLatestRustdeskInfoAsync(apiUrl);

            if (downloadUrl == null || version == null)
            {
                InstallationSettings.WriteToConsoleAndLog("Failed to get Rustdesk download information.");
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
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            else
            {
                InstallationSettings.WriteToConsoleAndLog("Rustdesk ID not found.");
            }

            if (!string.IsNullOrEmpty(InstallationSettings.rustdeskExe))
            {
                Installation.Cleanup(InstallationSettings.tempDir, InstallationSettings.rustdeskExe);
            }
            else
            {
                InstallationSettings.WriteToConsoleAndLog("Skipping cleanup, rustdeskExe is null or empty.");
            }
            InstallationSettings.ResetConsoleOutput();
        }
    }
}