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

                string responseData = response.Content.ReadAsStringAsync().Result;
                GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>(responseData);

                string downloadUrl = null;
                foreach (var asset in release.assets)
                {
                    if (asset.name.EndsWith(".exe"))
                    {
                        downloadUrl = asset.browser_download_url;
                        break;
                    }
                }

                return (downloadUrl, release.tag_name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Rustdesk release info: {ex.Message}");
                return (null, null);
            }
        }
    }
}
