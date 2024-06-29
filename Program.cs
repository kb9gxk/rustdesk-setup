using System;

namespace RustdeskSetup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var commandLineArgs = CommandLineArgs.Parse(args);

            if (commandLineArgs.ShouldShowHelp)
            {
                CommandLineArgs.ShowHelp();
                return;
            }

            // Set configuration from command line args or defaults
            Configuration.UseStableVersion = commandLineArgs.UseStableVersion;
            Configuration.RustdeskCfg = commandLineArgs.RustdeskCfg == "someConfigValue" ? null : commandLineArgs.RustdeskCfg;
            Configuration.RustdeskPw = commandLineArgs.RustdeskPw == "somePassword" ? null : commandLineArgs.RustdeskPw;

            // Logic to handle installation and configuration
            if (!string.IsNullOrEmpty(Configuration.RustdeskCfg) || !string.IsNullOrEmpty(Configuration.RustdeskPw))
            {
                Utility.ConfigureAndRunRustdesk();
            }
        }
    }
}
