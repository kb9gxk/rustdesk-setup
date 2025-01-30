using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class Installation
{
    internal static async Task DownloadAndInstallRustdeskAsync(string url, string tempDir)
    {
        InstallationSettings.log?.WriteLine($"Downloading latest {InstallationSettings.editionString} Rustdesk build...");

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
                        await installProcess.WaitForExitAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            InstallationSettings.log?.WriteLine($"Error downloading or installing {InstallationSettings.editionString} Rustdesk: {ex.Message}");
        }

        // Wait for 20 seconds
        await Task.Delay(20000);

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
                if (process != null)
                {
                    return process.StandardOutput.ReadToEnd().Trim();
                }
            }
        }
        catch (Exception ex)
        {
            InstallationSettings.log?.WriteLine($"Error getting Rustdesk ID: {ex.Message}");
            return null;
        }
        return null;
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
