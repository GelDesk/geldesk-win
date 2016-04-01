using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GelDesk.Configuration
{
    static class AppConfigJson
    {
        class JsonAppConfig
        {
            public string run { get; set; }
        }

        public static AppConfig ReadString(string json, string workingDirectory = null)
        {
            if (json == null)
                throw new ArgumentNullException("json");
            var model = new AppConfig();
            var data = JsonConvert.DeserializeObject<JsonAppConfig>(json);

            // Validate
            if (data.run == null)
                return model;
            data.run = data.run.Trim();
            if (data.run.Length == 0)
                return model;

            // Load Model
            var proc = CreateChildProcessConfigFromCommand(data.run, workingDirectory);
            model.ChildProcesses.Add(proc);

            return model;
        }

        public static AppConfig ReadFile(string file)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            file = file.Trim();
            file = Path.GetFullPath(file);
            var json = File.ReadAllText(file);
            var model = ReadString(json, Path.GetDirectoryName(file));
            model.File = file;
            return model;
        }

        static ChildProcessConfig CreateChildProcessConfigFromCommand(string commandLine, string workingDirectory = null)
        {
            var model = new ChildProcessConfig();
            var data = commandLine.SplitCommandFromArguments();
            if (data.Length > 0)
            {
                model.Command = data[0];
                model.WorkingDirectory = workingDirectory;
            }
            if (data.Length > 1)
                model.Arguments = data[1];
            return model;
        }
    }
}
