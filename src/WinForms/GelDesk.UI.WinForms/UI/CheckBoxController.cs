using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class CheckBoxController : ViewController<CheckBox>
    {
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new CheckBox();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            View.Text = ((string)ObjectData.GetValueOrDefault("text", Name));
            //_checkBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            View.SetDockStyle(ObjectData);
            View.UseVisualStyleBackColor = true;
            var checkedToken = ObjectData["checked"];
            if (checkedToken != null && checkedToken.Type == JTokenType.Boolean)
                View.Checked = (bool)checkedToken;
        }
    }
}
