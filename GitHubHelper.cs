// GitHubHelper.cs
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class GitHubHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();

        internal static async Task<(string downloadUrl, string version)> GetLatestRustdeskInfoAsync(string apiUrl)
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();
                string json = response.Content.ReadAsStringAsync().Result;

                GitHubRelease? release = JsonSerializer.Deserialize(json, MyJsonContext.Default.GitHubRelease);

                if (release?.assets == null)
                {
                    InstallationSettings.log?.WriteLine($"No {InstallationSettings.editionString} release found in GitHub response.");
                    return (null, null);
                }

                foreach (var asset in release.assets)
                {
                    if (asset.name.EndsWith("-x86_64.exe"))
                    {
                        string fileName = Path.GetFileName(asset.name);
                        string version = fileName.Split('-')[1];
                        return (asset.browser_download_url, version);
                    }
                }

                InstallationSettings.log?.WriteLine($"No {InstallationSettings.editionString} release found in GitHub response.");
                return (null, null);
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching {InstallationSettings.editionString} Rustdesk URL: {ex.Message}");
                return (null, null);
            }
        }
    }
}
