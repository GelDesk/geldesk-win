using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class BrowserController : ViewController<WebBrowser>
    {
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new WebBrowser();
            View.ScriptErrorsSuppressed = true;
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.SetDockStyle(ObjectData);
            View.MinimumSize = new System.Drawing.Size(20, 20);
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            var url = (string)ObjectData["url"];
            if (url != null)
                View.Url = new System.Uri(url, System.UriKind.Absolute);
        }

        protected override void OnLoadCompleted(RpcContext rpc)
        {
            if (View.Dock == DockStyle.Fill)
                View.BringToFront();
        }
    }
}
