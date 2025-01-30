// Configuration.cs

namespace RustdeskSetup
{
    internal static class Configuration
{
    private const string DefaultConfigValue = "";
    private const string DefaultPasswordValue = "";

    internal static bool? UseStableVersion { get; set; }
    internal static string RustdeskCfg { get; set; } = DefaultConfigValue;
    internal static string RustdeskPw { get; set; } = DefaultPasswordValue;
}
}
