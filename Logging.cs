using System;
using System.IO;

namespace RustdeskSetup
{
    internal static class Logging
    {
        internal static string logFilePath = $"c:\\Rustdesk-{InstallationSettings.editionString}-Install.log";
        internal static StreamWriter? log;

        internal static void RedirectConsoleOutput()
        {
            try
            {
                log = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
                log.WriteLine($"--- Rustdesk-{InstallationSettings.editionString} started at: {DateTime.Now} ---");
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
                log?.WriteLine($"--- Rustdesk-{InstallationSettings.editionString} ended at: {DateTime.Now} ---");
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
    }
}
