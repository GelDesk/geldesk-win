using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public static class EventExtensions
    {
        public static void Raise(this EventHandler handler, object sender, EventArgs e = null)
        {
            if (handler != null)
                handler(sender, e ?? EventArgs.Empty);
        }
    }
}
