using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;

namespace GelDesk
{
    /// <summary>
    /// A <see cref="IPlatformProvider"/> implementation for the WinForms platfrom.
    /// </summary>
    public class WinFormsPlatformProvider : IPlatformProvider, IDirectlyExecute
    {
        private SynchronizationContext dispatcher;
        private int uiThreadId;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsPlatformProvider"/> class.
        /// </summary>
        public WinFormsPlatformProvider()
        {
            dispatcher = CreateUIThreadContext();
            uiThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        static System.Threading.SynchronizationContext CreateUIThreadContext()
        {
            var context = SynchronizationContext.Current;
            if (!(context is WindowsFormsSynchronizationContext))
            {
                // If no context has been created yet, we have to first create
                // any control that has an hWnd. Then `SynchronizationContext
                // .Current` will return a WindowsFormsSynchronizationContext.
                var anyHwndControl = new Form();
                anyHwndControl.Name = "Default Marshalling Context";
                context = SynchronizationContext.Current;
                if (!(context is WindowsFormsSynchronizationContext))
                    throw new InvalidOperationException("Cannot create WindowsFormsSynchronizationContext.");
            }
            return context;

            // Not sure if this is better:

//            System.Threading.SynchronizationContext context = null;

//            // Let the Control class create the default WindowsFormsSynchronizationContext for us.
//            using (var control = new Control())
//                context = System.ComponentModel.AsyncOperationManager.SynchronizationContext;
//#if DEBUG
//            System.Diagnostics.Debug.Assert(context is WindowsFormsSynchronizationContext,
//                "System.Windows.Forms.Control.ctor should have instantiated a AsyncOperationManager.SynchronizationContext.");
//            GC.Collect();
//            System.Diagnostics.Debug.Assert(System.Threading.SynchronizationContext.Current is WindowsFormsSynchronizationContext,
//                "System.Windows.Forms.Control.ctor should have instantiated a AsyncOperationManager.SynchronizationContext.");
//#endif
//            return context;
        }

        /// <summary>
        /// Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public bool InDesignMode
        {
            get { return false; }
        }

        private void ValidateDispatcher()
        {
            if (dispatcher == null)
                throw new InvalidOperationException("Not initialized with dispatcher.");
        }

        private bool CheckAccess()
        {
            // I'm not sure how to implement this without having a control to 
            // reference. Perhaps we can save the control created in the 
            // `CreateUIThreadContext` method.
            return dispatcher == null 
                || (Application.OpenForms.Count > 0 
                    && !Application.OpenForms[0].InvokeRequired)
                || Thread.CurrentThread.ManagedThreadId == uiThreadId;

            // TODO: Check if the control created in CreateUIThreadContext gets disposed.
            // TODO: Is comparing ManagedThreadIds a reliable tactic? If so, 
            // maybe JUST do that instead of using Application.OpenForms.
        }

        public void BeginOnUIThread(SendOrPostCallback action, object value)
        {
            ValidateDispatcher();
            dispatcher.Post(action, value);
        }

        /// <summary>
        /// Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void BeginOnUIThread(System.Action action)
        {
            BeginOnUIThread((o) => action(), null);
        }

        /// <summary>
        /// Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns></returns>
        public Task OnUIThreadAsync(System.Action action)
        {
            ValidateDispatcher();
            var taskSource = new TaskCompletionSource<object>();
            SendOrPostCallback uiTaskMethod = (o) => {
                try
                {
                    action();
                    taskSource.SetResult(null);
                }
                catch (Exception ex)
                {
                    taskSource.SetException(ex);
                }
            };
            dispatcher.Post(uiTaskMethod, null);
            return taskSource.Task;
        }

        /// <summary>
        /// Executes the action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnUIThread(System.Action action)
        {
            if (CheckAccess())
            {
                action();
                return;
            }
            Exception exception = null;
            SendOrPostCallback uiMethod = (o) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            };
            dispatcher.Send(uiMethod, null);
            if (exception != null)
                throw new System.Reflection.TargetInvocationException("An error occurred while dispatching a call to the UI Thread", exception);
        }

        #region Not Implemented
        object IPlatformProvider.GetFirstNonGeneratedView(object view)
        {
            throw new NotImplementedException();
        }

        void IPlatformProvider.ExecuteOnFirstLoad(object view, Action<object> handler)
        {
            throw new NotImplementedException();
        }

        void IPlatformProvider.ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
            throw new NotImplementedException();
        }

        Action IPlatformProvider.GetViewCloseAction(object viewModel, ICollection<object> views, bool? dialogResult)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
