using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class WindowController : ViewController<Form>, IContainerObject
    {
        public WindowController()
        {
            OnRpc("close", Close);
        }
        
        void AddControls()
        {
            var controls = Components.ViewsOfType<Control>().Reverse();
            foreach (var control in controls)
            {
                View.Controls.Add(control);
                // Look for a Main Menu (the first MenuStrip that gets added).
                MenuStrip mainMnu = null;
                if (View.MainMenuStrip == null)
                {
                    mainMnu = control as MenuStrip;
                    if (mainMnu != null)
                        View.MainMenuStrip = mainMnu;
                }
            }
        }

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new Form();
            // Layout resumes at the end of OnLoad.
            View.SuspendLayout();
            // Set anything that should be set before children are added.
            View.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            View.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            View.ClientSize = new System.Drawing.Size(1008, 594);
        }

        protected override void OnLoad(RpcContext rpc)
        {
            AddControls();
            View.Name = Name;
            if (!View.SetStartPosition(ObjectData))
                View.StartPosition = FormStartPosition.CenterScreen;
            if (!View.SetTitleText(ObjectData))
                View.Text = Name;

            View.Shown += View_Shown;
            AddEventHandler("shown", rpc);

            View.FormClosing += View_FormClosing;
            AddEventHandler("closing", rpc);

            View.FormClosed += View_FormClosed;
            AddEventHandler("closed", rpc);

            View.ResizeEnd += View_ResizeEnd;
        }

        private void View_ResizeEnd(object sender, EventArgs e)
        {
            if (HasEventHandler("resized"))
                SendEvent("resized");
        }

        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_closing || !HasEventHandler("closing"))
                return;
            SendEvent("closing");
            e.Cancel = true;
            // CONSIDER: Should we really just cancel without further logic?
            // Currently, we rely on the fact that if there's a handler on the 
            // other side, it's going to call 'close' soon after this. (If it 
            // had already called 'close', we would have exited for _closing.)
        }

        protected override void OnLoadCompleted(RpcContext rpc)
        {
            View.ResumeLayout(false);
            View.PerformLayout();

            // if (data.visible)
            View.Show();
        }

        protected override void OnRemovedComponent(ComponentObject item)
        {
            Control ctrl;
            if (!item.TryGetView<Control>(out ctrl))
                return;
            View.Controls.Remove(ctrl);
        }

        bool _closing;
        void Close(RpcContext rpc)
        {
            _closing = true;
            rpc.Respond(true);
            View.Close();
        }

        void View_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (HasEventHandler("closed"))
                SendEvent("closed");
            if (Application.OpenForms.Count == 0)
                IoC.Get<ProcessManager>().Shutdown();
        }

        void View_Shown(object sender, EventArgs e)
        {
            if (HasEventHandler("shown"))
                SendEvent("shown");
        }
    }
}
