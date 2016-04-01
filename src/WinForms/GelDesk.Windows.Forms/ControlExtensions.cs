using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public static class ControlExtensions
    {
        #region DockStyle, Docking Splitter 
        public static Splitter AttachDockSplitter(this Control control, IList<Control> views, JObject data)
        {
            var dock = (string)data["dock"];
            if (dock == null)
                return null;
            var enabled = dock.EndsWith("-split");
            if (!enabled)
                return null;
            var splitter = new Splitter();
            views.Add(splitter);
            return splitter;
        }
        public static void LoadDockSplitter(this Control owner, Splitter splitter, JObject data)
        {
            if (splitter == null)
                return;
            splitter.Name = owner.Name + "Splitter";
            splitter.TabStop = false;
            switch (owner.Dock)
            {
                case DockStyle.None:
                    break;
                case DockStyle.Top:
                case DockStyle.Bottom:
                    splitter.Dock = owner.Dock;
                    splitter.Size = new System.Drawing.Size(100, 6);
                    break;
                case DockStyle.Left:
                case DockStyle.Right:
                    splitter.Dock = owner.Dock;
                    splitter.Size = new System.Drawing.Size(6, 100);
                    break;
                case DockStyle.Fill:
                    break;
            }
        }
        public static bool SetDockStyle(this Control control, JObject data,
            params Control[] attachedControls)
        {
            bool splitEnabled;
            if (!SetDockStyle(control, data, out splitEnabled))
                return false;
            foreach (var actrl in attachedControls)
                actrl.Dock = control.Dock;
            return true;
        }

        public static bool SetDockStyle(this Control control, JObject data, 
            Splitter splitter)
        {
            bool splitEnabled;
            if (!SetDockStyle(control, data, out splitEnabled))
                return false;
            if (splitEnabled)
                LoadDockSplitter(control, splitter, data);
            return true;
        }

        public static bool SetDockStyle(this Control control, JObject data, out bool splitEnabled)
        {
            var dock = (string)data?["dock"];
            if (dock == null)
            {
                splitEnabled = false;
                return false;
            }
            splitEnabled = dock.EndsWith("-split");
            if (splitEnabled)
                dock = dock.Substring(0, dock.Length - "-split".Length);
            var dockEnum = dock.ToEnum(DockStyle.None);
            control.Dock = dockEnum;
            return true;
        }
        #endregion

        #region Labels
        public static Label AttachLabel(this Control owner, List<Control> views, JObject data)
        {
            var label = new Label();
            views.Add(label);
            return label;
        }
        public static void LoadLabel(this Control owner, Label label, JObject data)
        {
            if (label == null)
                return;
            label.Name = owner.Name + "Label";
            //label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label.SetLabelText(data);
            label.SetSizeFromText();
            label.SetTextAlignDefault();
        }
        public static bool SetLabelText(this Control control, JObject data)
        {
            return SetText(control, data, "label");
        }
        public static bool SetSizeFromText(this Label control)
        {
            // TODO: Use the font size to figure out size when text exists.
            if (control.Text?.Length > 0)
                control.Size = new System.Drawing.Size(10, 18);
            else
                control.Size = new System.Drawing.Size(10, 6);
            return true;
        }
        #endregion

        #region Padding
        public static bool SetPadding(this Control control, JObject data)
        {
            var token = data["padding"];
            if (token == null)
                return false;
            switch (token.Type)
            {
                case JTokenType.Array:
                    var values = ((JArray)token)
                        .Select(t => (int)t)
                        .ToArray();
                    control.Padding = new Padding(
                        values[0],
                        values[1],
                        values[2],
                        values[3]);
                    return true;
                case JTokenType.Integer:
                    control.Padding = new Padding((int)token);
                    return true;
                case JTokenType.Float:
                    control.Padding = new Padding((int)((float)token));
                    return true;
            }
            return false;
        }
        #endregion

        #region PlaceholderText
        // See 
        // - http://stackoverflow.com/a/31309562
        // - http://stackoverflow.com/a/5450496
        // - http://stackoverflow.com/a/19273816
        public static bool SetPlaceholderText(this ComboBox control, JObject data)
        {
            var token = data["placeholder"];
            if (token == null)
                return false;
            control.SetPlaceholderText((string)token);
            return true;
        }
        public static bool SetPlaceholderText(this TextBox control, JObject data)
        {
            var token = data["placeholder"];
            if (token == null)
                return false;
            var ptb = control as PlaceholderTextBox;
            if (ptb != null)
                ptb.PlaceholderText = (string)token;
            else
                control.SetPlaceholderText((string)token);
            return true;
        }
        public static void SetPlaceholderText(this ComboBox combo, string placeholderText)
        {
            if (combo == null || placeholderText == null)
                return;
            if (!combo.IsHandleCreated)
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    combo.HandleCreated -= handler;
                    SetPlaceholderText(combo, placeholderText);
                };
                combo.HandleCreated += handler;
                return;
            }
            var handle = Native.GetComboBoxItemHandle(combo.Handle);
            Native.SendMessage(handle, Native.EM_SETCUEBANNER, (IntPtr)1, placeholderText);
        }
        public static void SetPlaceholderText(this TextBox textBox, string placeholderText)
        {
            if (textBox == null || placeholderText == null)
                return;
            if (!textBox.IsHandleCreated)
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    textBox.HandleCreated -= handler;
                    SetPlaceholderText(textBox, placeholderText);
                };
                textBox.HandleCreated += handler;
                return;
            }
            Native.SendMessage(textBox.Handle, Native.EM_SETCUEBANNER, (IntPtr)1, placeholderText);
        }
        #endregion

        #region Text, TextAlignment
        public static bool SetTitleText(this Control control, JObject data)
        {
            return SetText(control, data, "title");
        }
        public static bool SetText(this Control control, JObject data, string key = null)
        {
            var token = data[key ?? "text"];
            if (token == null)
                return false;
            control.Text = (string)token;
            return true;
        }
        public static bool SetTextAlignDefault(this Label control)
        {
            control.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            return true;
        }
        #endregion
        public static bool SetStartPosition(this Form control, JObject data)
        {
            var token = data["startPosition"];
            if (token != null)
            {
                switch ((string)token)
                {
                    case "center":
                        control.StartPosition = FormStartPosition.CenterScreen;
                        return true;
                    case "os":
                        control.StartPosition = FormStartPosition.WindowsDefaultBounds;
                        return true;
                }
            }
            return false;
        }
    }
}
