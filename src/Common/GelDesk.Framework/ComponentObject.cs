using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public abstract class ComponentObject : IHandleRpc
    {
        protected ComponentObject()
        {
            _handlers = new Dictionary<string, RpcHandler>();

            var container = this as IContainerObject;
            if (container != null)
                _components = new ComponentsCollection(container);
        }

        #region Basic Info (Name, ObjectData, ObjectPath, Parent)
        public string Name { get; private set; }
        protected JObject ObjectData { get; private set; }
        public string ObjectPath { get; private set; }
        public ComponentObject Parent { get; protected internal set; }
        #endregion

        #region Components
        readonly ComponentsCollection _components;
        public ComponentsCollection Components { get { return _components; } }

        public bool AddComponent(string instanceName, ComponentObject item)
        {
            if (_components == null)
                return false;
            _components.Add(instanceName, item);
            OnAddedComponent(instanceName, item);
            return true;
        }
        protected virtual void OnAddedComponent(string instanceName, ComponentObject item) { }
        protected virtual void OnRemovedComponent(ComponentObject item) { }
        public bool RemoveComponent(ComponentObject item)
        {
            if (_components != null && _components.Remove(item))
            {
                OnRemovedComponent(item);
                item.Parent = null;
                return true;
            }
            return false;
        }
        #endregion

        #region RPC
        readonly Dictionary<string, RpcHandler> _handlers;
        public virtual bool HandleRpc(RpcContext context)
        {
            RpcHandler handler;
            if (!_handlers.TryGetValue(context.ActionName, out handler))
                return false;
            handler(context);
            return true;
        }
        protected ComponentObject OnRpc(string methodName, RpcHandler handler)
        {
            if (handler != null)
                _handlers.Add(methodName, handler);
            else
                _handlers.Remove(methodName);
            return this;
        }
        #endregion

        #region Published Events

        readonly Dictionary<string, int> _handlerCount = new Dictionary<string, int>();
        protected EventPublisher EventPublisher { get; private set; }

        public virtual void AddEventHandler(string eventName, RpcContext handler)
        {
            var path = EventPath(eventName);
            EventPublisher.Subscribe(path, handler);
            var count = _handlerCount.GetValueOrDefault(eventName);
            _handlerCount[eventName] = count + 1;
        }
        public string EventPath(string eventName)
        {
            return RpcPath.Join(this.ObjectPath, eventName);
        }
        protected bool HasEventHandler(string eventName)
        {
            return _handlerCount.GetValueOrDefault(eventName) > 0;
        }
        public virtual void RemoveEventHandler(string eventName, RpcContext handler)
        {
            var path = EventPath(eventName);
            EventPublisher.Unsubscribe(path, handler);
            var count = _handlerCount.GetValueOrDefault(eventName);
            _handlerCount[eventName] = count - 1;
        }
        protected void SendEvent(string eventName, params JToken[] arguments)
        {
            EventPublisher.Publish(EventPath(eventName), arguments);
        }

        #endregion
        
        public void Initialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            Parent = (ComponentObject)info.Container;
            EventPublisher = rpc.EventPublisher;
            Name = info.InstanceName;
            ObjectPath = RpcPath.Join(info.Container?.ObjectPath, Name);
            InitializeObjectData(rpc, info);
            OnInitialize(rpc, info, reader);
        }
        void InitializeObjectData(RpcContext rpc, ComponentInfo info)
        {
            var data = info.ObjectData;
            Debug.Print("init: {0} '{1}' with: {2}",
                this.GetType().FullName,
                ObjectPath,
                data?.ToString(Newtonsoft.Json.Formatting.None) ?? "null");
            ObjectData = data;
            if (data == null)
                return;
            var eventNames = (JArray)data["e"];
            if (eventNames != null)
            {
                data.Remove("e");
                for (var i = 0; i < eventNames.Count; i++)
                {
                    var eventName = (string)eventNames[i];
                    AddEventHandler(eventName, rpc);
                }
            }
        }
        public void Load(RpcContext rpc)
        {
            OnLoad(rpc);
        }
        public void LoadCompleted(RpcContext rpc)
        {
            OnLoadCompleted(rpc);
        }
        protected virtual void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader) { }
        protected virtual void OnLoad(RpcContext rpc) { }
        protected virtual void OnLoadCompleted(RpcContext rpc) { }
    }
}
