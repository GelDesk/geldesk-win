using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using GelDesk.UI;

namespace GelDesk
{
    partial class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            _container = new SimpleContainer();
        }

        #region Configure

        Configuration.AppConfig _config;
        partial void ConfigureComponents();
        protected override void Configure()
        {
            RpcSerializerConfig.ApplyGlobalDefaultSerializerSettings();

            _config = Configuration.AppConfigCli.ReadArgs();
            _container.Instance(_config);

            RegisterSingletons();
            RegisterComponentTypes();
            ConfigureComponents();
        }
        void RegisterComponentTypes()
        {
            var componentType = typeof(ComponentObject);
            var kvps = GetBaseTypeRegistrations(componentType);
            foreach (var kvp in kvps)
            {
                Debug.Print("reg-type: {0} as '{1}'", 
                    kvp.Value.FullName, kvp.Key);
                _container.RegisterPerRequest(componentType, kvp.Key, kvp.Value);
            }
        }
        void RegisterSingletons()
        {
            _container.Singleton<RpcRouter>();
            _container.Singleton<EventPublisher>();
            _container.Singleton<ComponentManager>();
            _container.Singleton<ProcessManager>();
        }
        IEnumerable<KeyValuePair<string, Type>> GetBaseTypeRegistrations(Type baseType)
        {
            var ass = this.SelectAssemblies();
            //foreach (var item in ass)
            //    Debug.Print("Registering loadable components from assembly: {0}", item.FullName);
            var types = ass.SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !t.IsInterface
                    && (!t.IsNested || t.IsNestedPublic))
                .Select(t =>
                {
                    var namespaceName = t.Namespace.ToLower();
                    var typeName = t.Name;
                    if (typeName.EndsWith("Controller"))
                        typeName = typeName.Substring(0,
                            typeName.Length - "Controller".Length);
                    var key = namespaceName + "." + typeName;
                    return new KeyValuePair<string, Type>(key, t);
                });
            return types;
        }
        IEnumerable<KeyValuePair<string, Type>> GetInterfaceRegistrations(Type iface)
        {
            var ass = this.SelectAssemblies();
            //foreach (var item in ass)
            //    Debug.Print("Registering loadable components from assembly: {0}", item.FullName);
            var types = ass.SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(iface)
                    && !t.IsAbstract
                    && !t.IsInterface
                    && (!t.IsNested || t.IsNestedPublic))
                .Select(t =>
                {
                    var namespaceName = t.Namespace.ToLower();
                    var typeName = t.Name;
                    if (typeName.EndsWith("Controller"))
                        typeName = typeName.Substring(0,
                            typeName.Length - "Controller".Length);
                    var key = namespaceName + "." + typeName;
                    return new KeyValuePair<string, Type>(key, t);
                });
            return types;
        }

        #endregion

        #region IOC
        readonly SimpleContainer _container;

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;
            throw new InvalidOperationException("Could not locate any instances.");
        }
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }
        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
        #endregion

        #region Process Manager
        ProcessManager _processManager;
        bool _isProcessManagerExiting;

        protected override void OnStartup(object sender, EventArgs e)
        {
            _processManager = IoC.Get<ProcessManager>();
            _processManager.DoAppExit += process_DoAppExit;
            _processManager.Startup();
        }
        protected override void OnExit(object sender, EventArgs e)
        {
            if (_isProcessManagerExiting)
                return;
            _processManager.Shutdown();
        }
        void process_DoAppExit(object sender, EventArgs e)
        {
            _isProcessManagerExiting = true;
            Application.Exit();
        }
        #endregion
    }
}
