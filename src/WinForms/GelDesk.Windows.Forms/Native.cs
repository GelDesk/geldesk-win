using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    internal static class Native
    {
        private const uint ECM_FIRST = 0x1500;
        internal const uint EM_SETCUEBANNER = ECM_FIRST + 1;

        [DllImport("USER32.DLL")]
        static extern bool GetComboBoxInfo(IntPtr hwnd, ref COMBOBOXINFO pcbi);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, string lParam);

        [DllImport("USER32.DLL", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        #region ComboBox

        // See http://stackoverflow.com/a/31309562/16387

        [StructLayout(LayoutKind.Sequential)]
        struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public UInt32 stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        static COMBOBOXINFO GetComboBoxInfo(IntPtr handle)
        {
            var info = new COMBOBOXINFO();
            //a combobox is made up of three controls, a button, a list and textbox;
            //we want the textbox
            info.cbSize = Marshal.SizeOf(info);
            GetComboBoxInfo(handle, ref info);
            return info;
        }

        public static IntPtr GetComboBoxItemHandle(IntPtr comboBoxHandle)
        {
            var info = GetComboBoxInfo(comboBoxHandle);
            return info.hwndItem;
        }

        #endregion

    }
}
