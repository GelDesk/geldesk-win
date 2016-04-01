using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GelDesk
{
    /// <summary>
    /// A TextBox with a <see cref="PlaceholderText"/> property that works in 
    /// single or multi-line mode. See also 
    /// <see cref="ControlExtensions.SetPlaceholderText(TextBox, string)"/>.
    /// </summary>
    /// <remarks>
    /// From http://stackoverflow.com/a/19273816
    /// </remarks>
    [System.ComponentModel.DesignerCategory("Code")]
    public class PlaceholderTextBox : TextBox
    {
        public PlaceholderTextBox()
        {
            SetStyle(ControlStyles.UserPaint, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }
        
        #region Constants
        const int WM_PAINT = 0x000F;
        const int WM_PRINT = 0x0317;
        const int PRF_CLIENT = 0x00000004;
        const int PRF_ERASEBKGND = 0x00000008;
        #endregion

        Bitmap _bitmap;
        bool _paintedFirstTime;
        string _placeholderText;

        #region Properties
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set
            {
                if (_paintedFirstTime)
                    SetStyle(ControlStyles.UserPaint, false);
                base.BorderStyle = value;
                if (_paintedFirstTime)
                    SetStyle(ControlStyles.UserPaint, true);
            }
        }
        public override bool Multiline
        {
            get { return base.Multiline; }
            set
            {
                if (_paintedFirstTime)
                    SetStyle(ControlStyles.UserPaint, false);
                base.Multiline = value;
                if (_paintedFirstTime)
                    SetStyle(ControlStyles.UserPaint, true);
            }
        }
        protected override void OnFontChanged(EventArgs e)
        {
            if (_paintedFirstTime)
                SetStyle(ControlStyles.UserPaint, false);
            base.OnFontChanged(e);
            if (_paintedFirstTime)
                SetStyle(ControlStyles.UserPaint, true);
        }
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                Invalidate(true);
            }
        }
        #endregion

        #region Procedures
        protected override void Dispose(bool disposing)
        {
            if (_bitmap != null)
                _bitmap.Dispose();

            base.Dispose(disposing);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PAINT)
            {
                _paintedFirstTime = true;
                CaptureBitmap();
                SetStyle(ControlStyles.UserPaint, true);
                base.WndProc(ref m);
                SetStyle(ControlStyles.UserPaint, false);
                return;
            }

            base.WndProc(ref m);
        }
        void CaptureBitmap()
        {
            if (_bitmap != null)
                _bitmap.Dispose();

            _bitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(_bitmap))
            {
                int lParam = PRF_CLIENT | PRF_ERASEBKGND;

                var hdc = graphics.GetHdc();
                Native.SendMessage(this.Handle, WM_PRINT, hdc, (IntPtr)lParam);
                graphics.ReleaseHdc(hdc);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            SetStyle(ControlStyles.UserPaint, true);
            if (_bitmap == null)
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, ClientRectangle);
            else
                e.Graphics.DrawImageUnscaled(_bitmap, 0, 0);

            if (_placeholderText != null && Text.Length == 0)
                e.Graphics.DrawString(_placeholderText, Font, Brushes.Gray, -1f, 1f);

            SetStyle(ControlStyles.UserPaint, false);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Invalidate();
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }
        #endregion
    }
}
