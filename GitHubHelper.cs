using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class GitHubHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Regex versionRegex = new Regex(@"(\d+\.\d+\.\d+)", RegexOptions.Compiled);

        internal static async Task<(string? downloadUrl, string? version)> GetLatestRustdeskInfoAsync(string apiUrl)
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                using (HttpResponseMessage response = await httpClient.GetAsync(apiUrl))
                {
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();

                    GitHubRelease? release = JsonSerializer.Deserialize(json, MyJsonContext.Default.GitHubRelease);

                    if (release?.assets == null)
                    {
                        InstallationSettings.log?.WriteLine($"No {InstallationSettings.editionString} release found in GitHub response.");
                        return (null, null);
                    }

                    foreach (var asset in release.assets)
                    {
                        if (asset.name != null && asset.name.EndsWith("-x86_64.exe"))
                        {
                            string fileName = Path.GetFileName(asset.name);
                            Match match = versionRegex.Match(fileName);
                            if (match.Success)
                            {
                                string version = match.Groups[1].Value;
                                return (asset.browser_download_url, version);
                            }
                            else
                            {
                                InstallationSettings.log?.WriteLine($"Could not parse version from filename: {fileName}");
                                return (null, null);
                            }
                        }
                    }
                }

                InstallationSettings.log?.WriteLine($"No {InstallationSettings.editionString} release found in GitHub response.");
                return (null, null);
            }
            catch (HttpRequestException ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching {InstallationSettings.editionString} Rustdesk URL: {ex.Message}");
                return (null, null);
            }
             catch (JsonException ex)
            {
                 InstallationSettings.log?.WriteLine($"Error parsing JSON response: {ex.Message}");
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