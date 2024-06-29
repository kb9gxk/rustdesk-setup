using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using RustdeskSetup;

class Program
{
    static void Main(string[] args)
    {
        // Parse command line arguments
        var parsedArgs = CommandLineArgs.Parse(args);
        
        // Set up configuration
        bool useStableVersion = Configuration.UseStableVersion ?? parsedArgs.UseStableVersion;
        string rustdeskCfg = Configuration.RustdeskCfg ?? parsedArgs.RustdeskCfg;
        string rustdeskPw = Configuration.RustdeskPw ?? parsedArgs.RustdeskPw;
        
        if (rustdeskCfg == "someConfigValue") rustdeskCfg = string.Empty;
        if (rustdeskPw == "somePassword") rustdeskPw = string.Empty;

        // Log setup
        string editionString = useStableVersion ? "Stable" : "Nightly";
        string logFilePath = $"c:\\Rustdesk-{editionString}-Install.log";
        StreamWriter log = new StreamWriter(logFilePath, true);

        try
        {
            Console.SetOut(log);
            Console.SetError(log);
            
            log.WriteLine($"Rustdesk-{editionString} started at: {DateTime.Now}");

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

            log.WriteLine($"Rustdesk-{editionString} ended at: {DateTime.Now}");
        }
        catch (Exception ex)
        {
            log.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            log.Close();
        }
    }
}
