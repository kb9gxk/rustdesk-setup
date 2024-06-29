namespace RustdeskSetup
{
    internal static class Configuration
    {
        public static bool UseStableVersion { get; set; } = true; // Default to true
        public static string RustdeskCfg { get; set; } = "someConfigValue";
        public static string RustdeskPw { get; set; } = "somePassword";
    }
}
