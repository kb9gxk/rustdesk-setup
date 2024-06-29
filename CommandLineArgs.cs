using System;
using System.Diagnostics;

namespace RustdeskSetup
{
    internal class CommandLineArgs
    {
        internal bool UseStableVersion { get; private set; }
        internal string RustdeskCfg { get; private set; }
        internal string RustdeskPw { get; private set; }
        internal bool ShouldShowHelp { get; private set; }

        internal static CommandLineArgs Parse(string[] args)
        {
            var parsedArgs = new CommandLineArgs();
            bool useStableVersionSet = false;

            for (int i = 0; i < args.Length; i++)
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
                    parsedArgs.RustdeskCfg = args[i].Substring("--config=".Length).TrimStart('=');
                }
                else if (args[i].StartsWith("--password=", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.RustdeskPw = args[i].Substring("--password=".Length);
                }
                else if (args[i].Equals("--help", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArgs.ShouldShowHelp = true;
                    return parsedArgs;
                }
            }

            if (!useStableVersionSet)
            {
                string executableName = Process.GetCurrentProcess().ProcessName.ToLower();
                parsedArgs.UseStableVersion = !executableName.StartsWith("rustdesk-nightly");
            }

            return parsedArgs;
        }

        internal static void ShowHelp()
        {
            string executableName = Process.GetCurrentProcess().ProcessName.ToLower();
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
