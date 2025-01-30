using System;
using System.Diagnostics;
using System.IO;

namespace RustdeskSetup
{
    internal class CommandLineArgs // Removed 'static'
    {
        public bool UseStableVersion { get; set; }
        public string RustdeskCfg { get; set; } = "";
        public string RustdeskPw { get; set; } = "";
        public bool ShouldShowHelp { get; set; }

        internal static CommandLineArgs Parse() // 'static' keyword remains here
        {
            var parsedArgs = new CommandLineArgs();
            bool useStableVersionSet = false;
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 1; i < args.Length; i++) // Start from 1 to skip the executable path
            {
                if (args[i].Equals("--stable", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.UseStableVersion = true;
                    useStableVersionSet = true;
                }
                else if (args[i].Equals("--nightly", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.UseStableVersion = false;
                    useStableVersionSet = true;
                }
                else if (args[i].StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.RustdeskCfg = args[i].Substring("--config=".Length);
                }
                else if (args[i].StartsWith("--password=", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.RustdeskPw = args[i].Substring("--password=".Length);
                }
                else if (args[i].Equals("--help", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.ShouldShowHelp = true;
                    return parsedArgs; // Return immediately to show help
                }
                // Add additional argument handling as needed
            }

            // Default useStableVersion to true if not specified
            if (!useStableVersionSet)
            {
                parsedArgs.UseStableVersion = DetermineDefaultUseStableVersion();
            }

            return parsedArgs;
        }

        internal static bool DetermineDefaultUseStableVersion()
        {
            // Determine default based on executable name
            string executableName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName).ToLower();
            return !executableName.StartsWith("rustdesk-nightly");
        }

        internal static void ShowHelp()
        {
            string executableName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName).ToLower();
            bool isNightlyBuild = executableName.StartsWith("rustdesk-nightly");

            Console.WriteLine($"Usage: {executableName} [--stable|--nightly] [--config=<value>] [--password=<value>] [--help]");
            Console.WriteLine();

            if (isNightlyBuild)
            {
                Console.WriteLine("--stable           Use stable version of Rustdesk");
                Console.WriteLine("--nightly          Use nightly version of Rustdesk (Default)");
            }
            else
            {
                Console.WriteLine("--stable           Use stable version of Rustdesk (Default)");
                Console.WriteLine("--nightly          Use nightly version of Rustdesk");
            }

            Console.WriteLine("--config=<value>   Set network configuration output from the Network Settings");
            Console.WriteLine("--password=<value> Set the permanent password for Rustdesk");
            Console.WriteLine("--help             Show this help message");
        }
    }
}
