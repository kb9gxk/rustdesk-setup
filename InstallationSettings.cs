using System;
using System.IO;
using System.Net.Http;

namespace RustdeskSetup
{
    internal static class InstallationSettings
    {
        internal const string RustdeskInfoFileName = "rustdesk.txt";
        internal static string tempDir = Path.GetTempPath();
        internal static string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        internal static string editionString = Configuration.UseStableVersion.HasValue && Configuration.UseStableVersion.Value ? "Stable" : "Nightly";
        internal static string githubStableApiUrl = "https://api.github.com/repos/rustdesk/rustdesk/releases/latest";
        internal static string githubNightlyApiUrl = "https://api.github.com/repos/rustdesk/rustdesk/releases/tags/nightly";
        internal static string logFilePath = "C:\\Rustdesk-" + editionString + "-Install.log"; // Modified log path
        internal static string RustdeskInfoFilePath = "C:\\" + RustdeskInfoFileName; // Modified info path
        internal static StreamWriter? log;
        internal static string? rustdeskExe; // Will be dynamically set during installation
        internal static readonly HttpClient httpClient = new HttpClient();

        internal static void RedirectConsoleOutput()
        {
            try
            {
                log = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
                log.WriteLine($"--- Rustdesk-{editionString} started at: {DateTime.Now} ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error redirecting console output: {ex.Message}");
            }
        }

        internal static void ResetConsoleOutput()
        {
            try
            {
                log?.WriteLine($"--- Rustdesk-{editionString} ended at: {DateTime.Now} ---");
                log?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting console output: {ex.Message}");
            }
        }

         internal static void WriteToConsoleAndLog(string message)
        {
            Console.WriteLine(message);
            log?.WriteLine(message);
        }
    }
}