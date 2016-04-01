using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public interface IDirectlyExecute
    {
        void BeginOnUIThread(System.Threading.SendOrPostCallback action, object value);
    }
}
