namespace RustdeskSetup
{
    internal static class Configuration
    {
        private const string DefaultConfigValue = "";
        private const string DefaultPasswordValue = "";
        //This can be set via environment variables or a config file
        private const string DefaultJeffConfigValue = "";
        private const string DefaultJeffPasswordValue = "";

        internal static bool? UseStableVersion { get; set; } = true;
        internal static string RustdeskCfg { get; set; } = DefaultConfigValue;
        internal static string RustdeskPw { get; set; } = DefaultPasswordValue;

        internal static void SetJeffDefaults()
        {
            RustdeskCfg = DefaultJeffConfigValue;
            RustdeskPw = DefaultJeffPasswordValue;
            InstallationSettings.log?.WriteLine("Jeff Defaults Set.");
        }
    }
}