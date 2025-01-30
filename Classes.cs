using System.Text.Json.Serialization;

namespace RustdeskSetup
{
    public class Asset
    {
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("browser_download_url")]
        public string browser_download_url { get; set; }
    }

    [JsonSerializable(typeof(GitHubRelease))]
    public partial class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string tag_name { get; set; }
        [JsonPropertyName("assets")]
        public Asset[] assets { get; set; }
    }
}
