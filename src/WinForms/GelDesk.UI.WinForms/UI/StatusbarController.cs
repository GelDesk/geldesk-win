using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class StatusbarController : ViewController<StatusStrip>, IContainerObject
    {
        void AddStatusItems()
        {
            var items = Components.ViewsOfType<ToolStripItem>().ToArray();
            if (items.Length > 0)
                View.Items.AddRange(items);
        }
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new StatusStrip();
            // Layout resumes at the end of OnLoad.
            View.SuspendLayout();
            // Set anything that should be set before children are added.
        }
        protected override void OnLoad(RpcContext rpc)
        {
            AddStatusItems();
            View.Location = new System.Drawing.Point(0, 0);
            View.Name = Name;
            //View.Size = new System.Drawing.Size(284, 24);
            View.TabIndex = 1;
            View.Text = ((string)ObjectData.GetValueOrDefault("text", Name));
        }

        protected override void OnLoadCompleted(RpcContext rpc)
        {
            View.ResumeLayout(false);
            View.PerformLayout();
        }
        protected override void OnRemovedComponent(ComponentObject item)
        {
            ToolStripItem tool;
            if (!item.TryGetView<ToolStripItem>(out tool))
                return;
            View.Items.Remove(tool);
        }
    }
}
