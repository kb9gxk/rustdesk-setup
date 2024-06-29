using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace RustdeskSetup
{
    internal static class GitHubHelper
    {
        private static HttpClient httpClient = new HttpClient();

        internal static (string downloadUrl, string version) GetLatestRustdeskInfo(string apiUrl)
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();
                string json = response.Content.ReadAsStringAsync().Result;

                var release = JsonConvert.DeserializeObject<GitHubRelease>(json);

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
