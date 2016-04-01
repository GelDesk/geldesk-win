using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    partial class Bootstrapper
    {
        partial void ConfigureComponents()
        {
            BrowserUtils.ChangeUserAgent("Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko GelDesk/1.0");
        }
        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            // The base class selects it's own assembly and the caller's.
            // Here we are adding in the default UI kit and the default
            // Browser implementation (The WinForms WebBrowser control).
            // So the returned assemblies should be something like:
            // 
            // - geldesk.exe
            // - GelDesk.Windows.Forms.dll
            // - GelDesk.UI.WinForms.dll
            // - GelDesk.UI.WinFormsBrowser.dll
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
