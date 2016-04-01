using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GelDesk
{
    /// <summary>
    /// <see cref="System.Windows.Forms.ApplicationContext"/> implementation necessary to run the app without a 
    /// startup form and to inject shutdown logic.
    /// </summary>
    /// <remarks>
    /// - https://www.google.com/search?q=winforms+without+startup+form+ApplicationContext
    /// - https://msdn.microsoft.com/en-us/library/ms157901.aspx
    /// 
    /// Simply passing an ApplicationContext instance to Application.Run is 
    /// enough to start a WinForms app without a startup form. This custom 
    /// class is not necessary for that.
    /// 
    /// CONSIDER: Get rid of this class if it turns out to not be necessary.
    /// </remarks>
    public class WinFormsAppContext : ApplicationContext
    {
        public WinFormsAppContext() { }
    }
}
