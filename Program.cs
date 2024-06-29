using System;
using System.IO;
using RustdeskSetup;

class Program
{
    internal static bool useStableVersion = false; // This can remain here if needed globally
    internal static string rustdeskCfg = Configuration.DefaultRustdeskCfg; // Ensure these are accessible
    internal static string rustdeskPw = Configuration.DefaultRustdeskPw;

    static void Main(string[] args)
    {
        RedirectConsoleOutput();

        try
        {
            // Parse command line arguments
            var parsedArgs = CommandLineArgs.Parse(args);
            
            // Set up configuration based on command line arguments or defaults
            useStableVersion = Configuration.UseStableVersion ?? parsedArgs.UseStableVersion;
            rustdeskCfg = Configuration.RustdeskCfg ?? parsedArgs.RustdeskCfg;
            rustdeskPw = Configuration.RustdeskPw ?? parsedArgs.RustdeskPw;
            
            // Additional logic to handle specific configurations
            if (rustdeskCfg == "someConfigValue") rustdeskCfg = string.Empty;
            if (rustdeskPw == "somePassword") rustdeskPw = string.Empty;

            InitializeInstallation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Main method: {ex.Message}");
        }
        finally
        {
            ResetConsoleOutput();
        }
    }

    // Redirects console output to log file
    static void RedirectConsoleOutput()
    {
        try
        {
            Console.WriteLine($"--- Rustdesk started at: {DateTime.Now} ---");
            Console.WriteLine($"Using Rustdesk version: {InstallationSettings.Version}");
            Console.WriteLine($"Using Rustdesk URL: {InstallationSettings.DownloadUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error redirecting console output: {ex.Message}");
        }
    }

    // Resets console output and closes log file
    static void ResetConsoleOutput()
    {
        try
        {
            Console.WriteLine($"--- Rustdesk ended at: {DateTime.Now} ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting console output: {ex.Message}");
        }
    }

    // Initializes Rustdesk installation process
    static void InitializeInstallation()
    {
        HideWindow();

        // Use rustdeskPw if not null, otherwise use rustdeskCfg
        if (!string.IsNullOrEmpty(rustdeskPw))
        {
            ConfigureWithPassword();
        }
        else if (!string.IsNullOrEmpty(rustdeskCfg))
        {
            ConfigureWithoutPassword();
        }
        else
        {
            Console.WriteLine("No configuration parameters provided.");
        }

        // Other initialization and installation steps
        PerformInstallationSteps();
    }

    static void ConfigureWithPassword()
    {
        Console.WriteLine("Configuring with password...");
        // Implement configuration logic using rustdeskPw
    }

    static void ConfigureWithoutPassword()
    {
        Console.WriteLine("Configuring without password...");
        // Implement configuration logic using rustdeskCfg
    }

    // Hides the console window during installation
    static void HideWindow()
    {
        var handle = NativeMethods.GetConsoleWindow();
        NativeMethods.ShowWindow(handle, NativeMethods.SW_HIDE);
    }

    // Perform the actual installation steps
    static void PerformInstallationSteps()
    {
        // Log setup
        string editionString = useStableVersion ? "Stable" : "Nightly";
        string logFilePath = $"c:\\Rustdesk-{editionString}-Install.log";
        using (StreamWriter log = new StreamWriter(logFilePath, true))
        {
            try
            {
                Console.SetOut(log);
                Console.SetError(log);
                
                log.WriteLine($"=== Rustdesk-{editionString} started at: {DateTime.Now} ===");

                var rustdeskInfo = Utility.GetLatestRustdeskInfo(useStableVersion);
                if (rustdeskInfo.downloadUrl == null || rustdeskInfo.version == null)
                {
                    log.WriteLine("Failed to retrieve Rustdesk information. Exiting.");
                    return;
                }

                Utility.DownloadAndInstallRustdesk(rustdeskInfo.downloadUrl, rustdeskInfo.version);
                string rustdeskId = Utility.GetRustdeskId();
                Utility.ConfigureAndRunRustdesk(rustdeskId, rustdeskCfg, rustdeskPw);
                Utility.SaveRustdeskInfo(rustdeskId);
                Utility.DisplayPopup(rustdeskId, rustdeskInfo.version);
                Utility.Cleanup();

                log.WriteLine($"=== Rustdesk-{editionString} ended at: {DateTime.Now} ===");
            }
            catch (Exception ex)
            {
                log.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
