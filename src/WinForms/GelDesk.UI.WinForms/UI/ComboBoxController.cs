using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class ComboBoxController : CompositeViewController<Control>
    {
        Label _label;
        ComboBox _comboBox;

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            _comboBox = new ComboBox();
            _label = _comboBox.AttachLabel(Views, ObjectData);
            Views.Add(_comboBox);
        }
        protected override void OnLoad(RpcContext rpc)
        {

            _comboBox.Name = Name;
            _comboBox.SetText(ObjectData);
            _comboBox.SetPlaceholderText(ObjectData);
            _comboBox.SetDockStyle(ObjectData, _label);

            _comboBox.LoadLabel(_label, ObjectData);

            var items = (JArray)ObjectData["items"];
            if (items != null && items.Count > 0)
            {
                var itemStrings = items.Select(token => (string)token)
                    .ToArray();
                _comboBox.Items.AddRange(itemStrings);
            }
        }
    }
}
