using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk.Configuration
{
    public static class AppConfigCli
    {
        public static AppConfig ReadArgs()
        {
            // TODO: Use a proper command line arguments parser here.
            var args = Environment.GetCommandLineArgs();

            // If there is only one argument, it's this executable's file name.
            // So try looking for a config file in the current working directory.
            if (args.Length == 1)
                return AppConfigJson.ReadFile(SR.DefaultConfigFileName);

            var arg1 = args[1].IfNullOrWhitespace("").Trim();
            if (arg1.Equals("-json", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Load config from JSON piped into standard input.
                throw new NotImplementedException("Loading config from piped JSON has not yet been implemented.");
            }
            else if (args.Length > 2)
            {
                // TODO: Read more command line args to construct an instance of AppConfiguration.
                throw new NotImplementedException("Loading config from command line arguments has not yet implemented.");
            }
            else
            {
                // The only usable argument must be the filename.
                return AppConfigJson.ReadFile(arg1);
            }
            throw new InvalidOperationException("Could not load app configuration.");
        }
    }
}
