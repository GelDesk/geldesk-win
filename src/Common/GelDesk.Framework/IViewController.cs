using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public interface IViewController
    {
        IEnumerable<object> GetViews();
    }
}
