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
    public class PanelController : CompositeViewController<Control>, IContainerObject
    {
        Panel _panel;
        Splitter _splitter;
        
        void AddControls()
        {
            var controls = Components.ViewsOfType<Control>().Reverse();
            foreach (var control in controls)
            {
                _panel.Controls.Add(control);
            }
        }

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            _panel = new Panel();
            Views.Add(_panel);
            _splitter = _panel.AttachDockSplitter(Views, ObjectData);
            // Layout resumes at the end of OnLoad.
            _panel.SuspendLayout();
            //View.BackColor = System.Drawing.SystemColors.Window;
        }

        protected override void OnLoad(RpcContext rpc)
        {
            AddControls();
            _panel.Name = Name;
            _panel.SetDockStyle(ObjectData, _splitter);
            _panel.Size = new System.Drawing.Size(150, 100);
            _panel.SetPadding(ObjectData);
        }

        protected override void OnLoadCompleted(RpcContext rpc)
        {
            _panel.ResumeLayout(false);
            _panel.PerformLayout();
        }

        protected override void OnRemovedComponent(ComponentObject item)
        {
            Control ctrl;
            if (!item.TryGetView<Control>(out ctrl))
                return;
            _panel.Controls.Remove(ctrl);
        }
    }
}
