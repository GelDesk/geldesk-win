using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace GelDesk
{
    public static class DirectlyExecute
    {
        public static IDirectlyExecute Executor { get; set; } = new DefaultExecutor();

        /// <summary>
        ///   Executes the action on the UI thread asynchronously using a direct method.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void BeginOnUIThread(this System.Threading.SendOrPostCallback action, object value)
        {
            Executor.BeginOnUIThread(action, value);
        }

        class DefaultExecutor : IDirectlyExecute
        {
            public DefaultExecutor()
            {
                _context = SynchronizationContext.Current;
            }

            readonly SynchronizationContext _context;

            public void BeginOnUIThread(SendOrPostCallback action, object value)
            {
                _context.Post(action, value);
            }
        }
    }
}
