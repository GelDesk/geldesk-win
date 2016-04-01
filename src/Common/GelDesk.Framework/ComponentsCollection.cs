using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    // TODO: Inherit from KeyedCollection<String, ComponentObject> instead.
    public class ComponentsCollection : IEnumerable<ComponentObject>
    {
        public ComponentsCollection() : this(null) { }

        public ComponentsCollection(IContainerObject container)
        {
            //_container = container;
            _components = new Dictionary<string, ComponentObject>();
        }

        //readonly IContainerObject _container;

        #region Components

        readonly Dictionary<string, ComponentObject> _components;

        public IEnumerator<ComponentObject> GetEnumerator()
        {
            return _components.Values.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _components.Values.GetEnumerator();
        }

        public void Add(string instanceName, ComponentObject item)
        {
            _components.Add(instanceName, item);
        }

        public bool Remove(ComponentObject item)
        {
            if (item != null && _components.Remove(item.Name))
            {
                return true;
            }
            return false;
        }

        public bool TryGetByPath(string path, out ComponentObject component)
        {
            component = null;
            if (path == null)
                return false;
            var pathParts = RpcPath.Split(path);
            var collection = this;
            var instanceName = (string)null;
            for (var i = 0; i < pathParts.Length; i++)
            {
                instanceName = pathParts[i];
                if (!collection.TryGetByName(instanceName, out component))
                    break;
                if (i == pathParts.Length - 1)
                    return true;
                collection = (component as IContainerObject)?.Components;
                if (collection == null)
                    break;
            }
            return false;
        }

        public bool TryGetByName(string name, out ComponentObject component)
        {
            return _components.TryGetValue(name, out component);
        }

        #endregion
    }
}
