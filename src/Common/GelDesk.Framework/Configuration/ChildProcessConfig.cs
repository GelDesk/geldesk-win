using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk.Configuration
{
    public class ChildProcessConfig
    {
        public string Arguments { get; set; }
        public string Command { get; set; }
        public string Name { get; set; }
        public bool NativeConsoleWindow { get; set; }
        public string WorkingDirectory { get; set; }

    }
}
