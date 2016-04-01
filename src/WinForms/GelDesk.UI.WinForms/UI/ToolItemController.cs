using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class ToolItemController : ViewController<ToolStripItem>, IContainerObject
    {
        ObservableCollection<CommandItem> _commandItems;
        public string Group { get; private set; }
        public CommandItem Command { get; set; }
        public bool IsSubMenu { get; private set; }

        void AddMenuItems()
        {
            var dd = View as ToolStripDropDownItem;
            if (dd == null)
                return;
            // Select sub-items grouped by Group and add a null ToolStripItem 
            // placeholder to the end of each group. Flatten the list and then 
            // replace each null placeholder with a separator. Except the last
            // placeholder throw that one out.
            var items = Components.OfType<ToolItemController>()
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
            dd.DropDownItems.AddRange(items);
        }

        void BindToCommandItem(RpcContext rpc, ComponentInfo info, string binding)
        {
            var cmdSet = this.FindUp<CommandSet>();
            CommandItem cmd;
            if (cmdSet == null
                || !cmdSet.Root.TryGetItemByPath(binding, out cmd))
            {
                throw new InvalidOperationException("Invalid binding");
            }
            Command = cmd;
            _commandItems = cmd.Items;
            CreateSubItemsFromCommands(rpc, _commandItems, this);
        }

        void CreateSubItemsFromCommands(RpcContext rpc, IList<CommandItem> items, ComponentObject parent)
        {
            if (items == null)
                return;
            foreach (var item in items)
                CreateSubItemFromCommand(rpc, item, parent);
        }

        void CreateSubItemFromCommand(RpcContext rpc, CommandItem item, ComponentObject parent)
        {
            var name = item.Id;
            var menuItem = new ToolItemController();
            JObject data = null;
            var info = new ComponentInfo(menuItem,
                parent as IContainerObject, name, data);
            menuItem.Command = item;
            menuItem.Initialize(rpc, info, null);
            CreateSubItemsFromCommands(rpc, item.Items, menuItem);
            parent.AddComponent(name, menuItem);
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
            // Binding
            var bind = (string)ObjectData?["bind"];
            if (bind != null)
                BindToCommandItem(rpc, info, bind);

            if (reader?.HasChildren == true || Components?.Count() > 0)
                View = new ToolStripDropDownButton();
            else if (Parent.GetType() == typeof(ToolItemController))
                View = new ToolStripMenuItem();
            else
                View = new ToolStripButton();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            AddMenuItems();
            View.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            //View.Image = ???;
            //View.ImageTransparentColor = ???;
            View.Name = Name;
            //View.Size = new System.Drawing.Size(37, 20);

            if (Command != null)
            {
                View.Text = AccessText.Strip(Command.Text);
                View.Enabled = Command.Enabled;
                Command.PropertyChanged += Command_PropertyChanged;
            }
            else if (ObjectData != null)
            {
                View.Enabled = ((bool?)ObjectData["enabled"]).GetValueOrDefault(true);
                View.Text = AccessText.Strip(
                    (string)ObjectData.GetValueOrDefault("text", Name));
            }
            // TODO: Set AutoToolTip to true if DisplayStyle excludes text.
            View.AutoToolTip = false;

            if (!IsSubMenu)
                View.Click += View_Click;
            //AddEventHandler("clicked", rpc);
        }

        void Command_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "enabled":
                    View.Enabled = Command.Enabled;
                    break;
                case "items":
                    // TODO: Re-bind to new Command.Items.
                    //_commandItems = Command.Items;
                    break;
                case "text":
                    View.Text = AccessText.Strip(Command.Text);
                    break;
            }
        }

        void View_Click(object sender, EventArgs e)
        {
            if (Command != null)
                Command.Execute();
            if (HasEventHandler("clicked"))
                SendEvent("clicked");
        }

        protected override void OnRemovedComponent(ComponentObject item)
        {
            var dd = View as ToolStripDropDownItem;
            if (dd == null)
                return;
            ToolStripItem tool;
            if (!item.TryGetView<ToolStripItem>(out tool))
                return;
            dd.DropDownItems.Remove(tool);
        }
    }
}
