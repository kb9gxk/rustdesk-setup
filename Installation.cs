using System;
using System.Diagnostics;
using System.IO;

namespace RustdeskSetup
{
    internal static class Installation
    {
        internal static void DownloadAndInstallRustdesk(string url, string tempDir)
        {
            InstallationSettings.log?.WriteLine($"Downloading latest {InstallationSettings.editionString} Rustdesk build...");

            try
            {
                var response = InstallationSettings.httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();

                string fileName = Path.GetFileName(new Uri(url).LocalPath);
                string exePath = Path.Combine(tempDir, fileName);

                using (var fileStream = File.OpenWrite(exePath))
                {
                    response.Content.CopyToAsync(fileStream).Wait();
                }

                InstallationSettings.rustdeskExe = fileName;

                var installProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--silent-install",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                installProcess?.WaitForExit();
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error downloading or installing {InstallationSettings.editionString} Rustdesk: {ex.Message}");
            }

            System.Threading.Thread.Sleep(20000); // Wait for 20 seconds

            try
            {
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "RustDesk.lnk"));
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error deleting desktop shortcut: {ex.Message}");
            }
        }

        internal static string GetRustdeskDirectory()
        {
            return Path.Combine(InstallationSettings.programFilesDir, "RustDesk");
        }

        internal static string GetRustdeskExecutable(string rustdeskDir)
        {
            return Path.Combine(rustdeskDir, InstallationSettings.rustdeskExe);
        }

        internal static string GetRustdeskId(string runMe, string rustdeskDir)
        {
            InstallationSettings.log?.WriteLine("Getting Rustdesk ID...");
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
                    return process?.StandardOutput.ReadToEnd().Trim();
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error getting Rustdesk ID: {ex.Message}");
                return null;
            }
        }

        internal static void Cleanup(string tempDir, string exeName)
        {
            try
            {
                File.Delete(Path.Combine(tempDir, exeName));
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error cleaning up temp directory: {ex.Message}");
            }
        }
    }
}
