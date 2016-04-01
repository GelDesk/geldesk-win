using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class StatusItemController : ViewController<ToolStripStatusLabel>
    {
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new ToolStripStatusLabel();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            View.Spring = true;
            View.Text = ((string)ObjectData.GetValueOrDefault("text", Name));
            View.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }
    }
}
