using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using CefSharp;
using CefSharp.WinForms;

namespace GelDesk.UI
{
    public class BrowserController : ViewController<ChromiumWebBrowser>
    {
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            var url = (string)ObjectData["url"];
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                url = "about:blank";
            View = new ChromiumWebBrowser(url);
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.SetDockStyle(ObjectData);
            View.MinimumSize = new System.Drawing.Size(20, 20);
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            var url = (string)ObjectData["url"];
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                View.Load(url);
        }

        protected override void OnLoadCompleted(RpcContext rpc)
        {
            if (View.Dock == DockStyle.Fill)
                View.BringToFront();
        }
    }
}
