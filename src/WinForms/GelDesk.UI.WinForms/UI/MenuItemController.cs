using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class MenuItemController : ViewController<ToolStripMenuItem>, IContainerObject
    {
        public string Group { get; private set; }
        public CommandItem Command { get; set; }
        public bool IsSubMenu { get; private set; }

        void AddMenuItems()
        {
            // Select sub-items grouped by Group and add a null ToolStripItem 
            // placeholder to the end of each group. Flatten the list and then 
            // replace each null placeholder with a separator. Except the last
            // placeholder throw that one out.
            var items = Components.OfType<MenuItemController>()
                .GroupBy(item => item.Group)
                .SelectMany(grouping =>
                {
                    return grouping.SelectMany(item =>
                        item.GetViews().OfType<ToolStripItem>()
                    ).Concat(new ToolStripItem[] { null });
                })
                .ToArray();
            var count = items.Length - 1;
            if (count < 1)
                return;
            IsSubMenu = true;
            for (var i = 0; i < count; i++)
                if (items[i] == null)
                    items[i] = new ToolStripSeparator();
            items = items.Take(count).ToArray();
            View.DropDownItems.AddRange(items);
        }
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            if (Command != null)
            {
                Group = Command.Group;
            }
            else
            {
                Group = (string)ObjectData["group"];
            }
            View = new ToolStripMenuItem();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            AddMenuItems();
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);
            if (Command != null)
            {
                View.Text = AccessText.Convert(Command.Text);
                View.Enabled = Command.Enabled;
                Command.PropertyChanged += Command_PropertyChanged;
            }
            else if (ObjectData != null)
            {
                View.Enabled = ((bool?)ObjectData["enabled"]).GetValueOrDefault(true);
                View.Text = AccessText.Convert(
                    (string)ObjectData.GetValueOrDefault("text", Name));
            }
            if (!IsSubMenu)
                View.Click += View_Click;
        }

        void Command_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        private void View_Click(object sender, EventArgs e)
        {
            if (Command != null)
                Command.Execute();
            if (HasEventHandler("clicked"))
                SendEvent("clicked");
        }

        protected override void OnRemovedComponent(ComponentObject item)
        {
            ToolStripItem tool;
            if (!item.TryGetView<ToolStripItem>(out tool))
                return;
            View.DropDownItems.Remove(tool);
        }
    }
}
