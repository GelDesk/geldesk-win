using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class TextBoxController : CompositeViewController<Control>
    {
        Label _label;
        TextBox _textBox;

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            _textBox = (ObjectData["placeholder"] != null 
                && (bool?)ObjectData["multiline"] == true) ?
                new PlaceholderTextBox() :
                new TextBox();
            _label = _textBox.AttachLabel(Views, ObjectData);
            Views.Add(_textBox);
        }
        protected override void OnLoad(RpcContext rpc)
        {
            _textBox.Name = Name;
            _textBox.SetText(ObjectData);
            _textBox.SetPlaceholderText(ObjectData);
            _textBox.SetDockStyle(ObjectData, _label);
            //_textBox.TextAlign = HorizontalAlignment.Left;

            _textBox.LoadLabel(_label, ObjectData);
        }
    }
}
