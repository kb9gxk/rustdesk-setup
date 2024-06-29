using System;
using System.IO;

namespace RustdeskSetup
{
    internal static class InstallationSettings
    {
        internal static string TempDir = Path.GetTempPath();
        internal static string ProgramFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        internal static string EditionString = Configuration.UseStableVersion ? "Stable" : "Nightly";
        internal static string GitHubStableApiUrl = "https://api.github.com/repos/rustdesk/rustdesk/releases/latest";
        internal static string GitHubNightlyApiUrl = "https://api.github.com/repos/rustdesk/rustdesk/releases/tags/nightly";
        internal static string LogFilePath = $"c:\\Rustdesk-{EditionString}-Install.log";
        internal static StreamWriter? Log;
        internal static string RustdeskExe; // Will be dynamically set during installation
        internal static System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();

        internal static void RedirectConsoleOutput()
        {
            try
            {
                Log = new StreamWriter(LogFilePath, append: true) { AutoFlush = true };
                Log.WriteLine($"--- Rustdesk-{EditionString} started at: {DateTime.Now} ---");
                Console.SetOut(Log);
                Console.SetError(Log);
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
                Log?.WriteLine($"--- Rustdesk-{EditionString} ended at: {DateTime.Now} ---");
                var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(standardOutput);
                Console.SetError(standardOutput);
                Log?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting console output: {ex.Message}");
            }
        }

        internal static void HideWindow()
        {
            var handle = NativeMethods.GetConsoleWindow();
            NativeMethods.ShowWindow(handle, NativeMethods.SW_HIDE);
        }
    }
}
