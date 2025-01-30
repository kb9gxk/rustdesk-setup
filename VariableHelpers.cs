// VariableHelpers.cs
using System.Text.Json.Serialization;

namespace RustdeskSetup
{
    [JsonSerializable(typeof(GitHubRelease))]
    public partial class MyJsonContext : JsonSerializerContext
    {
    }
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string tag_name { get; set; }
        [JsonPropertyName("assets")]
        public Asset[] assets { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("browser_download_url")]
        public string browser_download_url { get; set; }
    }
}
