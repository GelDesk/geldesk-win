using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk.Configuration
{
    public class AppConfig
    {
        public string File { get; set; }

        readonly List<ChildProcessConfig> _childProcesses =
            new List<ChildProcessConfig>();
        public IList<ChildProcessConfig> ChildProcesses
        {
            get { return _childProcesses; }
        }
    }
}
