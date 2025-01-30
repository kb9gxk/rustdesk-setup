// Configuration.cs

namespace RustdeskSetup
{
    internal static class Configuration
    {
        private const string DefaultConfigValue = "someConfigValue";
        private const string DefaultPasswordValue = "somePassword";
        internal static bool? useStableVersion = null;
        internal static string rustdeskCfg = DefaultConfigValue;
        internal static string rustdeskPw = DefaultPasswordValue;
    }
}
