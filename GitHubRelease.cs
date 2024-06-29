namespace RustdeskSetup
{
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public Asset[] assets { get; set; }
    }

    public class Asset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
    }
}
