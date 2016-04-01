using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GelDesk.Configuration;
using Caliburn.Micro;

namespace GelDesk
{
    public sealed class ProcessManager
    {
        public ProcessManager(AppConfig config, ComponentManager components)
        {
            _config = config;
            var controller = new ProcessManagerController(this);
            components.AddComponent("process", controller);
        }

        #region Configure and Startup

        readonly AppConfig _config;

        public void Startup()
        {
            var router = IoC.Get<RpcRouter>();
            router.Initialize();

            LoadChildProcesses();
            StartupChildProcesses();
        }

        #endregion

        #region Shutdown

        bool _shuttingDown;

        public event EventHandler DoAppExit;

        /// <summary>
        /// Begins shutting down child processes. When the last child process 
        /// stops, <see cref="DoAppExit"/> is called.
        /// </summary>
        public void Shutdown()
        {
            if (_shuttingDown)
                return;
            _shuttingDown = true;
            ShutdownChildProcesses();
        }

        void OnShutdownCompleted()
        {
            DoAppExit.Raise(this);
        }

        #endregion

        #region Child Processes

        List<ChildProcess> _childProcesses;

        void ChildProcess_Started(object sender, EventArgs e)
        {
            var proc = (ChildProcess)sender;
            proc.Connection.Notify("process/connected");
        }

        void ChildProcess_Stopped(object sender, EventArgs e)
        {
            var proc = (ChildProcess)sender;
            proc.Started -= ChildProcess_Started;
            proc.Stopped -= ChildProcess_Stopped;
            _childProcesses.Remove(proc);
            if (_childProcesses.Count == 0)
                OnShutdownCompleted();
        }

        void LoadChildProcesses()
        {
            var procs = _config.ChildProcesses
                .Select(config => new ChildProcess(config))
                .ToArray();
            _childProcesses = new List<ChildProcess>(procs);
            foreach (var proc in procs)
            {
                proc.Started += ChildProcess_Started;
                proc.Stopped += ChildProcess_Stopped;
            }
        }

        void ShutdownChildProcesses()
        {
            var procs = _childProcesses.ToArray();
            foreach (var proc in procs)
            {
                _childProcesses.Remove(proc);
                proc.Stop();
            }
        }

        void StartupChildProcesses()
        {
            var procs = _childProcesses.ToArray();
            foreach (var proc in procs)
                proc.Start();
        }

        #endregion

        #region Controller
        class ProcessManagerController : ComponentObject
        {
            public ProcessManagerController(ProcessManager processMgr)
            {
                _mgr = processMgr;

                OnRpc("quit", Quit);
            }

            readonly ProcessManager _mgr;

            void Quit(RpcContext rpc)
            {
                rpc.Respond(true);
                _mgr.Shutdown();
            }
        }
        #endregion
    }
}
