using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class Installation
    {
        internal static async Task DownloadAndInstallRustdeskAsync(string url, string tempDir)
        {
            InstallationSettings.WriteToConsoleAndLog($"Downloading latest {InstallationSettings.editionString} Rustdesk build...");

            try
            {
                using (var response = await InstallationSettings.httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    string fileName = Path.GetFileName(new Uri(url).LocalPath);
                    string exePath = Path.Combine(tempDir, fileName);

                    using (var fileStream = File.OpenWrite(exePath))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    InstallationSettings.rustdeskExe = fileName;

                    using (var installProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = "--silent-install",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }))
                    {
                         if (installProcess != null)
                        {
                            // Use a CancellationTokenSource for timeout
                            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
                            {
                                try
                                {
                                    await installProcess.WaitForExitAsync(cts.Token);
                                }
                                catch (TaskCanceledException)
                                {
                                    InstallationSettings.WriteToConsoleAndLog("Rustdesk installation timed out.");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error downloading {InstallationSettings.editionString} Rustdesk: {ex.Message}");
            }
            catch (IOException ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error writing file: {ex.Message}");
            }
            catch (Exception ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error downloading or installing {InstallationSettings.editionString} Rustdesk: {ex.Message}");
            }

            // Wait for 20 seconds
            await Task.Delay(20000);

            try
            {
                string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "RustDesk.lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error deleting desktop shortcut: {ex.Message}");
            }
        }

        internal static string? GetRustdeskId(string runMe, string rustdeskDir)
        {
            InstallationSettings.WriteToConsoleAndLog("Getting Rustdesk ID...");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = runMe,
                Arguments = "--get-id",
                WorkingDirectory = rustdeskDir,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                     if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            return process.StandardOutput.ReadToEnd().Trim();
                        }
                        else
                        {
                            InstallationSettings.WriteToConsoleAndLog($"Error getting Rustdesk ID: Process exited with code {process.ExitCode}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error getting Rustdesk ID: {ex.Message}");
                return null;
            }
            return null;
        }

        internal static void Cleanup(string tempDir, string exeName)
        {
            try
            {
                string exePath = Path.Combine(tempDir, exeName);
                if (File.Exists(exePath))
                {
                    File.Delete(exePath);
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.WriteToConsoleAndLog($"Error cleaning up temp directory: {ex.Message}");
            }
        }

        internal static string GetRustdeskDirectory()
        {
            // Default Rustdesk installation directory
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RustDesk");
        }

        internal static string GetRustdeskExecutable(string rustdeskDir)
        {
            // Default Rustdesk executable name
            return Path.Combine(rustdeskDir, "rustdesk.exe");
        }
    }
}
