using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;

namespace GelDesk
{
    /// <summary>
    /// Inherit from this class in order to customize the configuration of the framework.
    /// </summary>
    public abstract class BootstrapperBase
    {
        bool _initialized;

        public ApplicationContext AppContext { get; protected set; }

        /// <summary>
        /// Initialize the framework.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            AppContext = new WinFormsAppContext();
            var winformsProvider = new WinFormsPlatformProvider();
            PlatformProvider.Current = winformsProvider;
            DirectlyExecute.Executor = winformsProvider;
            if (Execute.InDesignMode)
                throw new NotImplementedException("Support for design-time is not implemented.");
            
            StartRuntime();
            // Since there is no Application.Startup event in WinForms, we 
            // simply call the OnStartup method right here. 
            OnStartup(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called by Initialize to start the framework at runtime.
        /// </summary>
        protected virtual void StartRuntime()
        {
            //AssemblySourceCache.Install();
            //AssemblySource.Instance.AddRange(SelectAssemblies());

            PrepareApplication();

            Configure();
            IoC.GetInstance = GetInstance;
            IoC.GetAllInstances = GetAllInstances;
            IoC.BuildUp = BuildUp;
        }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUnhandledException;
            Application.ApplicationExit += OnExit;
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure() { }
        
        /// <summary>
        /// Override to tell the framework where to find assemblies to inspect for views, etc.
        /// </summary>
        /// <returns>A list of assemblies to inspect.</returns>
        protected virtual IEnumerable<System.Reflection.Assembly> SelectAssemblies()
        {
            return new[] {
                // The Framework assembly.
                typeof(CommandSet).Assembly,
                // This assembly.
                typeof(BootstrapperBase).Assembly,
                // The assembly of the subclass type.
                GetType().Assembly
            };
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected virtual object GetInstance(Type service, string key)
        {
            //if (service == typeof(IWindowManager))
            //    service = typeof(WindowManager);
            return Activator.CreateInstance(service);
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected virtual IEnumerable<object> GetAllInstances(Type service)
        {
            return new[] { Activator.CreateInstance(service) };
        }

        /// <summary>
        /// Override this to provide an IoC specific implementation.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected virtual void BuildUp(object instance) { }

        /// <summary>
        /// Override this to add custom behavior to execute after the application starts.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        protected virtual void OnStartup(object sender, EventArgs e) { }

        /// <summary>
        /// Override this to add custom behavior on exit.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnExit(object sender, EventArgs e) { }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(object sender, System.Threading.ThreadExceptionEventArgs e) { }
    }
}
