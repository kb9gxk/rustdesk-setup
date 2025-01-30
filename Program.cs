using System;
using System.IO;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal class Program
    {
        [STAThread] // Required for MessageBox to work correctly
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
            Configuration.RustdeskCfg = parsedArgs.RustdeskCfg;
            Configuration.RustdeskPw = parsedArgs.RustdeskPw;

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
            
            Installation.Cleanup(InstallationSettings.tempDir, InstallationSettings.rustdeskExe);
            InstallationSettings.ResetConsoleOutput();
        }
    }
}
