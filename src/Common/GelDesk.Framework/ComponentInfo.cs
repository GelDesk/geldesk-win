using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public sealed class ComponentInfo
    {
        public ComponentInfo(ComponentObject component,
            IContainerObject container, string name, JObject data)
        {
            Component = component;
            Container = container;
            InstanceName = name;
            ObjectData = data;
            Parent = container as ComponentObject;
        }
        public ComponentObject Component { get; }
        public IContainerObject Container { get; }
        public string InstanceName { get; }
        public JObject ObjectData { get; }
        public ComponentObject Parent { get; }
    }
}
