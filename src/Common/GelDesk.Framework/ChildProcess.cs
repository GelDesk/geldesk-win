using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GelDesk.Configuration;

namespace GelDesk
{
    public sealed class ChildProcess
    {
        public ChildProcess(ChildProcessConfig config)
        {
            _config = config;
        }
        readonly ChildProcessConfig _config;
        Process _process;

        #region Create
        void CreateProcessForNativeWindow()
        {
            _process = new Process();
            _process.StartInfo = new ProcessStartInfo(_config.Command, _config.Arguments)
            {
                // http://stackoverflow.com/questions/5255086/when-do-we-need-to-set-useshellexecute-to-true
                UseShellExecute = true,
                WorkingDirectory = _config.WorkingDirectory,
                CreateNoWindow = false,
                ErrorDialog = true,

                // CONSIDER: Figure out the least hackish way to implement ShowConsoleOnStart for a native console
                // since it may never be shown if WindowStyle starts out as hidden. See 
                // http://stackoverflow.com/questions/2647820/toggle-process-startinfo-windowstyle-processwindowstyle-hidden-at-runtime
                //
                // Therefore, we can't do this:
                //
                // WindowStyle = (_config.ShowConsoleOnStart ? 
                //    ProcessWindowStyle.Normal : 
                //    ProcessWindowStyle.Hidden)
                //
                // To work around this, the native console would probably have to be shown temporarily and then hidden
                // ASAP (if the model had ShowConsoleOnStart = false and NativeConsoleWindow = true).
                //
                // So for now, we simply don't support that...
                //
                WindowStyle = ProcessWindowStyle.Normal
            };
        }

        void CreateProcessForStdio()
        {
            _process = new Process();
            _process.StartInfo = new ProcessStartInfo(_config.Command, _config.Arguments)
            {
                // http://stackoverflow.com/questions/5255086/when-do-we-need-to-set-useshellexecute-to-true
                UseShellExecute = false,
                WorkingDirectory = _config.WorkingDirectory,
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            _process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
        }

        #endregion

        #region Start

        public event EventHandler Started;

        public void Start()
        {
            if (_config.NativeConsoleWindow)
                CreateProcessForNativeWindow();
            else
                CreateProcessForStdio();

            _process.EnableRaisingEvents = true;
            _process.Exited += process_Exited;

            if (_config.NativeConsoleWindow)
            {
                StartProcessForNativeWindow();
                Started.Raise(this);
            }
            else
            {
                StartProcessForStdio();
                InitializeConnection();
                OpenConnection();
            }
        }

        void StartProcessForNativeWindow()
        {
            _process.Start();
        }

        void StartProcessForStdio()
        {
            _process.Start();
            //
            // NOTE: StandardInput sends CR+LF (\r\n) line endings by default,
            // on Windows. If this becomes necessary to change, here is how
            // (and where) you would do that:
            //
            // proc.StandardInput.NewLine = "\n";
            //
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
        }

        #endregion

        #region Stop

        public event EventHandler Stopped;

        void process_Exited(object sender, EventArgs e)
        {
            Stopped.Raise(this);
        }
        public void Stop()
        {
            _connection.Notify("process/shutdown");
            if (!_process.WaitForExit(1024))
                _process.Kill();
        }

        #endregion

        #region Standard I/O
        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            Debug.Print("proc-err: " + e.Data);

            // TODO: Send received data to console/log.
            // TODO: React to child process errors per configuration or script handler.
        }

        void process_NonRpcOutputDataReceived(object channel, string data)
        {
            Debug.Print("proc-in: " + data);
        }

        #endregion

        #region RPC

        RpcConnection _connection;
        public RpcConnection Connection { get { return _connection; } }

        void InitializeConnection()
        {
            if (_config.NativeConsoleWindow)
                return;
            var channel = new RpcStdioChannel(_process);
            channel.ReadNonRpc(process_NonRpcOutputDataReceived);
            _connection = new RpcConnection(channel);
        }

        void OnConnectionOpened(Task task)
        {
            Started.Raise(this);

            // Dev automation code:
            //System.Threading.Thread.Sleep(1000);
            //_rpc.Send(new GelDesk.Messages.Beep()).ContinueWith(tr =>
            //{
            //    _rpc.Send(new GelDesk.Messages.Boop()).ContinueWith(tr2 =>
            //    {
            //        _rpc.Send(new GelDesk.Messages.Beep()).ContinueWith(tr3 =>
            //        {
            //            System.Threading.Thread.Sleep(1000);
            //            Stop();
            //        });
            //    });
            //});
        }

        void OpenConnection()
        {
            var cn = _connection;
            if (cn == null)
                return;
            var startTask = cn.Open();
            if (startTask != null)
                startTask.ContinueWith(OnConnectionOpened);
            else
                OnConnectionOpened(null);
        }

        void CloseConnection()
        {
            var cn = _connection;
            if (cn == null)
                return;
            _connection = null;
            // TODO: Before closing cn, send a notice.
            //cn.Send("parentProcessExit");
            cn.Close();
        }

        #endregion
    }
}
