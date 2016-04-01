using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class ButtonController : ViewController<Button>
    {
        public CommandItem Command { get; set; }

        void BindToCommandItem(string binding)
        {
            var cmdSet = this.FindUp<CommandSet>();
            CommandItem cmd;
            if (cmdSet == null
                || !cmdSet.Root.TryGetItemByPath(binding, out cmd))
            {
                throw new InvalidOperationException("Invalid binding");
            }
            Command = cmd;
        }

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new Button();

            // Binding
            var bind = (string)ObjectData?["bind"];
            if (bind != null)
                BindToCommandItem(bind);
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            if (Command != null)
            {
                View.Enabled = Command.Enabled;
                View.Text = AccessText.Convert(Command.Text);
                Command.PropertyChanged += Command_PropertyChanged;
            }
            else if (ObjectData != null)
            {
                View.Text = ((string)ObjectData.GetValueOrDefault("text", Name));
            }
            View.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            View.AutoSize = true;
            View.Padding = new Padding(3);
            View.SetDockStyle(ObjectData);
            View.UseVisualStyleBackColor = true;

            View.Click += View_Click;
        }

        private void View_Click(object sender, EventArgs e)
        {
            if (Command != null)
                Command.Execute();
            if (HasEventHandler("click"))
                SendEvent("click");
        }

        private void Command_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "enabled":
                    View.Enabled = Command.Enabled;
                    break;
                case "text":
                    View.Text = AccessText.Convert(Command.Text);
                    break;
            }
        }
    }
}
