using System;
using System.Diagnostics;
using System.IO;

namespace RustdeskSetup
{
    internal class CommandLineArgs
    {
        public bool UseStableVersion { get; set; }
        public string RustdeskCfg { get; set; } = "";
        public string RustdeskPw { get; set; } = "";
        public bool ShouldShowHelp { get; set; }
        public bool IsJeffBuild { get; set; }

        internal static CommandLineArgs Parse()
        {
            var parsedArgs = new CommandLineArgs();
            bool useStableVersionSet = false;
            string[] args = Environment.GetCommandLineArgs();
            string executableName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName).ToLower();
            parsedArgs.IsJeffBuild = executableName.Contains("-jeff");

            for (int i = 1; i < args.Length; i++)
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
                    string configValue = args[i].Substring("--config=".Length);
                    if (!string.IsNullOrWhiteSpace(configValue))
                    {
                        parsedArgs.RustdeskCfg = configValue;
                    }
                    else
                    {
                        Console.WriteLine("Error: --config argument requires a value.");
                        parsedArgs.ShouldShowHelp = true;
                        return parsedArgs;
                    }
                }
                else if (args[i].StartsWith("--password=", StringComparison.OrdinalIgnoreCase))
                {
                    string passwordValue = args[i].Substring("--password=".Length);
                   if (!string.IsNullOrWhiteSpace(passwordValue))
                    {
                        parsedArgs.RustdeskPw = passwordValue;
                    }
                    else
                    {
                        Console.WriteLine("Error: --password argument requires a value.");
                        parsedArgs.ShouldShowHelp = true;
                        return parsedArgs;
                    }
                }
                else if (args[i].Equals("--help", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.ShouldShowHelp = true;
                    return parsedArgs;
                }
            }

            if (!useStableVersionSet)
            {
                parsedArgs.UseStableVersion = DetermineDefaultUseStableVersion();
            }

            return parsedArgs;
        }

        internal static bool DetermineDefaultUseStableVersion()
        {
            string executableName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName).ToLower();
            return !executableName.StartsWith("rustdesk-nightly") && !executableName.Contains("-jeff");
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