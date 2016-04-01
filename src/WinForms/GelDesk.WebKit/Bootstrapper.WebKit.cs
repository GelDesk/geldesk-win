using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;

namespace GelDesk
{
    partial class Bootstrapper
    {
        partial void ConfigureComponents()
        {
            Cef.EnableHighDPISupport();

            var settings = new CefSettings();
            if (!Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: true))
                throw new Exception("Unable to Initialize CEF");
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            // The base class selects it's own assembly and the caller's.
            // Here we are adding in the default UI kit and the WebKit/CEF 
            // Browser implementation (CefSharp).
            // So the returned assemblies should be something like:
            // 
            // - geldesk.exe
            // - GelDesk.Windows.Forms.dll
            // - GelDesk.UI.WinForms.dll
            // - GelDesk.UI.WebKitBrowser.dll
            //
            return base.SelectAssemblies().Concat(new[]
            {
                typeof(GelDesk.UI.WindowController).Assembly,
                typeof(GelDesk.UI.BrowserController).Assembly
            });
            // TODO: Include third-party plug-in assemblies from geldesk.json.
        }
    }
}
