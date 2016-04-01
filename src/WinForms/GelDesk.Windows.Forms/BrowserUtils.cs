using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public static class BrowserUtils
    {
        #region ChangeUserAgent

        // See:
        // - https://msdn.microsoft.com/en-us/library/ms775125(v=vs.85).aspx
        // - http://stackoverflow.com/questions/937573/changing-the-user-agent-of-the-webbrowser-control/18080830#18080830
        // 

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

        const int URLMON_OPTION_USERAGENT = 0x10000001;
        const int URLMON_OPTION_USERAGENT_REFRESH = 0x10000002;

        static string _currentUA;

        /// <summary>
        /// Changes the User-Agent header value that is sent from every 
        /// <see cref="System.Windows.Forms.WebBrowser"/> control in the 
        /// process.
        /// </summary>
        /// <param name="ua"></param>
        public static void ChangeUserAgent(string ua)
        {
            //string ua = "Googlebot/2.1 (+http://www.google.com/bot.html)";

            if (ua == _currentUA)
                return;

            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT_REFRESH, null, 0, 0);
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, ua, ua.Length, 0);

            _currentUA = ua;
        }

        #endregion
    }
}
