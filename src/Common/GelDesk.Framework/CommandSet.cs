using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public class CommandSet : ComponentObject
    {
        public CommandSet()
        {
            _root = new CommandItem(this);
        }

        readonly CommandItem _root;

        public CommandItem Root { get { return _root; } }

        public void Execute(CommandItem item)
        {
            SendEvent("exec", item.ItemPath);
        }

        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            while (reader.MoveNext())
                _root.AddItem(reader.Current);
        }

        protected override void OnLoad(RpcContext rpc)
        {
            AddEventHandler("exec", rpc);
        }
    }
}
