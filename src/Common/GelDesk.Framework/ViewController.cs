using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Caliburn.Micro;

namespace GelDesk
{
    public abstract class ViewController : ComponentObject, IViewController
    {
        public abstract IEnumerable<object> GetViews();
    }
    public abstract class ViewController<TView> : ViewController
        where TView : class
    {
        protected TView View { get; set; }

        public override IEnumerable<object> GetViews()
        {
            return new object[] { View };
        }
    }
    public abstract class CompositeViewController<TView> : ViewController
        where TView : class
    {
        protected List<TView> Views { get; } = new List<TView>(1);

        public override IEnumerable<object> GetViews()
        {
            return Views.ToArray();
        }
    }
}
