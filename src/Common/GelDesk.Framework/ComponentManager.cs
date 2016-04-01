using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public sealed class ComponentManager
    {
        public ComponentManager(EventPublisher eventPublisher, RpcRouter rpc)
        {
            _components = new ComponentsCollection();
            _eventPublisher = eventPublisher;

            _router = new ComponentRouter(_components);
            rpc.AddHandler(_router);

            var controller = new ComponentManagerController(this);
            AddComponent("component", controller);
        }

        readonly ComponentsCollection _components;
        readonly EventPublisher _eventPublisher;
        readonly ComponentRouter _router;

        public void AddComponent(string instanceName, ComponentObject component)
        {
            _components.Add(instanceName, component);
        }

        #region Initialize & Load Component
        bool InitializeAndLoadComponent(RpcContext rpc, JArray data)
        {
            rpc.EventPublisher = _eventPublisher;
            var rootItem = InitializeComponent(rpc, data, null);
            LoadComponent(rpc, rootItem);
            AddComponent(rootItem.Name, rootItem);
            LoadCompletedComponent(rpc, rootItem);
            return true;
        }
        ComponentObject InitializeComponent(RpcContext rpc, JArray data, IContainerObject container)
        {
            using (var reader = new ComponentFrameReader(data))
            {
                // Parse
                if (!reader.MoveNext())
                    throw new InvalidOperationException("Couldn't read component header.");
                var header = ((string)reader.Current).SplitWith(':');
                var instanceName = header[0];
                var namespaceAndType = header[1];
                // Initialize Component
                var component = IoC.Get<ComponentObject>(namespaceAndType);
                if (component == null)
                    throw new InvalidOperationException("Type not found: {0}".FormatSafe(namespaceAndType));
                JObject objectData = null;
                if (reader.MoveNext())
                    objectData = (JObject)reader.Current;
                var info = new ComponentInfo(component, container, 
                    instanceName, objectData);
                component.Initialize(rpc, info, reader);
                // Initialize Children
                if (reader.MoveNext())
                {
                    var componentAsContainer = component as IContainerObject;
                    if (componentAsContainer == null)
                        throw new InvalidOperationException(
                            "Component type cannot have children: {0}"
                                .FormatSafe(namespaceAndType));
                    do
                        InitializeComponent(rpc, (JArray)reader.Current, componentAsContainer);
                    while (reader.MoveNext());
                }
                // Add To Parent
                var parent = (ComponentObject)container;
                if (parent != null)
                    parent.AddComponent(instanceName, component);
                // Finally...
                return component;
            }
        }
        void LoadComponent(RpcContext rpc, ComponentObject component)
        {
            // Process the childrent first, then the parent so that each 
            // parent can add it's own children to itself.
            var children = (component as IContainerObject)?.Components;
            if (children != null)
            {
                foreach (var child in children)
                    LoadComponent(rpc, child);
            }
            // Parent
            component.Load(rpc);
        }
        void LoadCompletedComponent(RpcContext rpc, ComponentObject component)
        {
            // Process the childrent first, then the parent so that each 
            // child can resume layout before it's parent.
            var children = (component as IContainerObject)?.Components;
            if (children != null)
            {
                foreach (var child in children)
                    LoadCompletedComponent(rpc, child);
            }
            // Parent
            component.LoadCompleted(rpc);
        }

        #endregion

        #region Controller
        class ComponentManagerController : ComponentObject
        {
            public ComponentManagerController(ComponentManager mgr)
            {
                _mgr = mgr;

                OnRpc("load", LoadComponent);
                OnRpc("subscribe", Subscribe);
                OnRpc("unsubscribe", Unsubscribe);
            }

            readonly ComponentManager _mgr;

            void LoadComponent(RpcContext rpc)
            {
                var result = _mgr.InitializeAndLoadComponent(rpc, rpc.Message.Arguments);
                rpc.Respond(result);
            }
            ComponentObject GetSubscriptionTarget(RpcContext rpc, out string eventName)
            {
                var args = rpc.Message.Arguments;
                if (args.Count < 2)
                    throw new InvalidOperationException("Requires 2 arguments.");
                var targetPath = (string)args[0];
                eventName = (string)args[1];
                ComponentObject component;
                if (!_mgr._components.TryGetByPath(targetPath, out component))
                    throw new InvalidOperationException("Target not found.");
                return component;
            }
            void Subscribe(RpcContext rpc)
            {
                string eventName;
                var component = GetSubscriptionTarget(rpc, out eventName);
                component.AddEventHandler(eventName, rpc);
            }

            void Unsubscribe(RpcContext rpc)
            {
                string eventName;
                var component = GetSubscriptionTarget(rpc, out eventName);
                component.RemoveEventHandler(eventName, rpc);
            }
        }
        #endregion
    }
}
