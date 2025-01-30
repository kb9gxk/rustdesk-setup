using System;
using System.Diagnostics;
using System.IO;

namespace RustdeskSetup
{
    internal static class Utility
    {
        internal static void ConfigureAndRunRustdesk(string rustdeskId, string runMe, string rustdeskCfg, string rustdeskPw)
        {
            InstallationSettings.log?.WriteLine("Configuring and starting Rustdesk...");

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = runMe;

                    // Strip any leading '=' from rustdeskCfg
                    if (!string.IsNullOrEmpty(rustdeskCfg) && rustdeskCfg.StartsWith("="))
                    {
                        rustdeskCfg = rustdeskCfg.Substring(1); // Remove the leading '='
                    }

                    var arguments = $"--config {rustdeskCfg}";
                    if (!string.IsNullOrEmpty(rustdeskPw))
                    {
                        arguments += $" --password {rustdeskPw}";
                    }

                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    // Removed the wait for exit
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error configuring or starting Rustdesk: {ex.Message}");
            }
        }

        internal static void SaveRustdeskInfo(string rustdeskId)
        {
            try
            {
                InstallationSettings.log?.WriteLine($"Computer: {Environment.MachineName}");
                InstallationSettings.log?.WriteLine($"ID: {rustdeskId}");
                File.WriteAllText(InstallationSettings.RustdeskInfoFilePath, $"Computer: {Environment.MachineName}\nID: {rustdeskId}");
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error saving Rustdesk info: {ex.Message}");
            }
        }

        internal static void DisplayPopup(string rustdeskId, string version)
        {
            try
            {
                NativeMethods.MessageBox(IntPtr.Zero, $"Computer: {Environment.MachineName}\nID: {rustdeskId}\nVersion: {version}",
                                         $"{InstallationSettings.editionString} Rustdesk Installer", 0);
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error displaying popup: {ex.Message}");
            }
        }
    }
}
