// Program.cs
using System;
using System.Diagnostics;
using System.IO;

namespace RustdeskSetup
{
    internal class Program
    {
        static async Task Main(string[] args)
{
    InstallationSettings.RedirectConsoleOutput();
    try
    {
        var commandLineArgs = CommandLineArgs.Parse();

        if (commandLineArgs.ShouldShowHelp)
        {
            CommandLineArgs.ShowHelp();
            return;
        }

        bool useStableVersion = commandLineArgs.UseStableVersion ?? CommandLineArgs.DetermineDefaultUseStableVersion();

        Configuration.rustdeskCfg = string.IsNullOrEmpty(commandLineArgs.RustdeskCfg) ? Configuration.rustdeskCfg == "someConfigValue" ? string.Empty : Configuration.rustdeskCfg : commandLineArgs.RustdeskCfg;
        Configuration.rustdeskPw = string.IsNullOrEmpty(commandLineArgs.RustdeskPw) ? Configuration.rustdeskPw == "somePassword" ? string.Empty : Configuration.rustdeskPw : commandLineArgs.RustdeskPw;

        await InitializeAsync(useStableVersion);
    }
    catch (Exception ex)
    {
        InstallationSettings.log?.WriteLine($"Error in Main method: {ex.Message}");
    }
    finally
    {
        InstallationSettings.ResetConsoleOutput();
    }
}

        static async Task InitializeAsync(bool useStableVersion)
        {
            InstallationSettings.HideWindow();
        
            string editionString = useStableVersion ? "Stable" : "Nightly";
            string githubApiUrl = useStableVersion ? InstallationSettings.githubStableApiUrl : InstallationSettings.githubNightlyApiUrl;
            InstallationSettings.editionString = editionString;
            InstallationSettings.logFilePath = $"c:\\Rustdesk-{editionString}-Install.log";
        
            (string rustdeskUrl, string version) = await GitHubHelper.GetLatestRustdeskInfoAsync(githubApiUrl);
        
            if (string.IsNullOrEmpty(rustdeskUrl) || string.IsNullOrEmpty(version))
            {
                InstallationSettings.log?.WriteLine($"Failed to retrieve {editionString} Rustdesk information. Exiting.");
                return;
            }
        
            InstallationSettings.log?.WriteLine($"Using Rustdesk {editionString} version: {version}");
            InstallationSettings.log?.WriteLine($"Using Rustdesk {editionString} URL: {rustdeskUrl}");
        
            await Installation.DownloadAndInstallRustdeskAsync(rustdeskUrl, InstallationSettings.tempDir); // Await the download
        
            string rustdeskDir = Installation.GetRustdeskDirectory();
            string runMe = Installation.GetRustdeskExecutable(rustdeskDir);
            string rustdeskId = Installation.GetRustdeskId(runMe, rustdeskDir);
        
            if (!string.IsNullOrEmpty(Configuration.rustdeskCfg))
            {
                Utility.ConfigureAndRunRustdesk(rustdeskId, runMe, Configuration.rustdeskCfg, Configuration.rustdeskPw);
            }
        
            Utility.SaveRustdeskInfo(rustdeskId);
            Utility.DisplayPopup(rustdeskId, version);
        
            Installation.Cleanup(InstallationSettings.tempDir, InstallationSettings.rustdeskExe);
        }
    }
}
