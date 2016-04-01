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
    public class MenuController : ViewController<MenuStrip>, IContainerObject
    {
        ObservableCollection<CommandItem> _commandItems;
        void AddMenuItems()
        {
            var items = Components.ViewsOfType<ToolStripItem>().ToArray();
            if (items.Length > 0)
                View.Items.AddRange(items);
        }

        void BindToCommandSet(RpcContext rpc, ComponentInfo info, string binding)
        {
            var cmdSet = info.Container?.Components
                .OfType<CommandSet>()
                .FirstOrDefault(item => item.Name == binding);
            if (cmdSet == null)
                throw new InvalidOperationException("Invalid binding");
            _commandItems = cmdSet.Root.Items;
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
            var menuItem = new MenuItemController();
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
            View = new MenuStrip();
            // Layout resumes at the end of OnLoad.
            View.SuspendLayout();
            // Set anything that should be set before children are added.

            // Binding
            var bind = (string)ObjectData["bind"];
            if (bind != null)
                BindToCommandSet(rpc, info, bind);
        }
        protected override void OnLoad(RpcContext rpc)
        {
            AddMenuItems();
            View.Location = new System.Drawing.Point(0, 0);
            View.Name = Name;
            //View.Size = new System.Drawing.Size(284, 24);
            View.TabIndex = 0;
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
