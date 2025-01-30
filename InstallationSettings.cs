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
    internal static string logFilePath = $"c:\\Rustdesk-{editionString}-Install.log";
    internal static string RustdeskInfoFilePath = Path.Combine("c:\\", RustdeskInfoFileName);
    internal static StreamWriter? log;
    internal static string? rustdeskExe; // Will be dynamically set during installation
    internal static readonly HttpClient httpClient = new HttpClient();

    internal static void RedirectConsoleOutput()
    {
        try
        {
            log = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
            log.WriteLine($"--- Rustdesk-{editionString} started at: {DateTime.Now} ---");
            Console.SetOut(log);
            Console.SetError(log);
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
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
            Console.SetError(standardOutput);
            log?.Dispose();
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
