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
                // Configure Rustdesk with the provided config (if any)
                if (!string.IsNullOrEmpty(rustdeskCfg))
                {
                    // Strip any leading '=' from rustdeskCfg
                    if (rustdeskCfg.StartsWith("="))
                    {
                        rustdeskCfg = rustdeskCfg.Substring(1); // Remove the leading '=' as it's not required
                    }
                    
                    InstallationSettings.log?.WriteLine($"Setting Rustdesk config: {rustdeskCfg}");
                    using (var configProcess = new Process())
                    {
                        configProcess.StartInfo.FileName = runMe;
                        configProcess.StartInfo.Arguments = $"--config {rustdeskCfg}";
                        configProcess.StartInfo.UseShellExecute = false;
                        configProcess.Start();
                        configProcess.WaitForExit(); // Wait for config to be set

                        if (configProcess.ExitCode != 0)
                        {
                            InstallationSettings.log?.WriteLine($"Error setting Rustdesk config: Process exited with code {configProcess.ExitCode}");
                        }
                        else
                        {
                            InstallationSettings.log?.WriteLine("Rustdesk config set successfully.");
                        }
                    }
                }

                // Configure Rustdesk with the provided password (if any)
                if (!string.IsNullOrEmpty(rustdeskPw))
                {
                    InstallationSettings.log?.WriteLine($"Setting Rustdesk password.");
                    using (var passwordProcess = new Process())
                    {
                        passwordProcess.StartInfo.FileName = runMe;
                        passwordProcess.StartInfo.Arguments = $"--password {rustdeskPw}";
                        passwordProcess.StartInfo.UseShellExecute = false;
                        passwordProcess.Start();
                        passwordProcess.WaitForExit(); // Wait for password to be set

                        if (passwordProcess.ExitCode != 0)
                        {
                            InstallationSettings.log?.WriteLine($"Error setting Rustdesk password: Process exited with code {passwordProcess.ExitCode}");
                        }
                        else
                        {
                            InstallationSettings.log?.WriteLine("Rustdesk password set successfully.");
                        }
                    }
                }


                // Start Rustdesk normally after configuration
                InstallationSettings.log?.WriteLine("Starting Rustdesk normally.");
                using (var startProcess = new Process())
                {
                    startProcess.StartInfo.FileName = runMe;
                    startProcess.StartInfo.UseShellExecute = false;
                    startProcess.Start();

                    if (startProcess.HasExited)
                    {
                        InstallationSettings.log?.WriteLine($"Error starting Rustdesk: Process exited immediately.");
                    }
                    else
                    {
                        InstallationSettings.log?.WriteLine("Rustdesk started successfully.");
                    }
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