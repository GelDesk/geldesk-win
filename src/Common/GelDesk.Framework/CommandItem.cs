using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public class CommandItem : PropertyChangedBase, INotifyPropertyChanged
    {
        public CommandItem(CommandSet owner) { _owner = owner; }
        public CommandItem(CommandSet owner, JToken token) : this(owner, token, null) { }
        public CommandItem(CommandSet owner, JToken token, CommandItem parent) : this(owner)
        {
            var data = (JArray)token;
            Id = (string)data.FirstOrDefault();
            ItemPath = RpcPath.Join(parent?.ItemPath, Id);
            System.Diagnostics.Debug.Print("init: CommandItem {0}", ItemPath);
            if (data.Count > 1)
                InitProperties((JObject)data[1]);
            if (data.Count > 2)
                InitItems(data, 2);
        }

        bool _enabled = true;
        string _group;
        string _id;
        ObservableCollection<CommandItem> _items;
        readonly CommandSet _owner;
        string _text;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                NotifyOfPropertyChange("Enabled");
            }
        }
        public string Group
        {
            get { return _group; }
            set
            {
                if (_group == value)
                    return;
                _group = value;
                NotifyOfPropertyChange("group");
            }
        }
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == value)
                    return;
                _id = value;
                NotifyOfPropertyChange("Id");
            }
        }
        public string ItemPath { get; private set; }
        public ObservableCollection<CommandItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyOfPropertyChange("Items");
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                NotifyOfPropertyChange("Text");
            }
        }

        void InitItems(JArray data, int start = 0)
        {
            Items = new ObservableCollection<CommandItem>();
            for (var i = start; i < data.Count; i++)
                AddItem(data[i]);
        }
        void InitProperties(JObject props)
        {
            if (props == null)
                return;
            JToken value = null;
            value = props["enabled"];
            if (value != null)
                Enabled = (bool)value;
            value = props["group"];
            if (value != null)
                Group = (string)value;
            value = props["text"];
            if (value != null)
                Text = (string)value;
        }
        public void AddItem(JToken token)
        {
            var item = new CommandItem(_owner, token, this);
            EnsureItems();
            Items.Add(item);
        }
        void EnsureItems()
        {
            if (_items == null)
                _items = new ObservableCollection<CommandItem>();
        }
        public void Execute()
        {
            _owner.Execute(this);
        }
        public CommandItem GetItem(string id)
        {
            CommandItem item;
            TryGetItem(id, out item);
            return item;
        }
        public CommandItem GetItemByPath(string path)
        {
            CommandItem item;
            TryGetItemByPath(path, out item);
            return item;
        }
        public bool TryGetItem(string id, out CommandItem item)
        {
            item = Items.FirstOrDefault(i => i.Id == id);
            return item != null;
        }
        public bool TryGetItemByPath(string path, out CommandItem item)
        {
            item = null;
            if (path == null)
                return false;
            var pathPart = RpcPath.Split(path);
            var parent = this;
            var id = (string)null;
            for (var i = 0; i < pathPart.Length; i++)
            {
                id = pathPart[i];
                if (!parent.TryGetItem(id, out item))
                    break;
                if (i == pathPart.Length - 1)
                    return true;
                parent = item;
            }
            return false;
        }
    }
}
