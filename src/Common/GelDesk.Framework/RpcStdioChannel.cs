using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public class RpcStdioChannel : IRpcChannel
    {
        readonly Process _process;
        readonly StreamWriter _output;

        public RpcStdioChannel(Process process)
        {
            // This channel begins in the opened state.
            _process = process;
            _output = process.StandardInput;
            _process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            _opened = true;
        }

        #region Open

        bool _opened;
        readonly object _openGuard = new object();
        public Task Open()
        {
            lock (_openGuard)
            {
                if (!_opened)
                    _opened = true;
            }
            return null;
        }

        #endregion

        #region Close
        public void Close()
        {
            lock (_openGuard)
            {
                if (!_opened)
                    return;
                _opened = false;
            }
            _output.Flush();
        }

        #endregion

        #region Read

        RpcDataHandler _readHandler = DefaultOnRead;
        Action<object, string> _readNonRpcHandler = DefaultOnReadNonRpc;

        static void DefaultOnRead(object sender, string data)
        {
            throw new InvalidOperationException("An RpcDataHandler has not been supplied to IRpcChannel.Read.");
        }

        static void DefaultOnReadNonRpc(object sender, string data) { }
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;

            // Null Data? - This will happen once when a child process exits.
            if (data == null)
            {
                Debug.Print("rpc-stdio: null data received.");
                return;
            }
            // Non-RPC Data?
            if (!data.StartsWith(SR.RpcIncomingPrefix))
            {
                _readNonRpcHandler(this, data);
                return;
            }

            // RPC Data.
            if (!_opened)
            {
                Debug.Print("rpc-stdio: input discarded (closed) - " + data);
                return;
            }
            data = data.Substring(SR.RpcIncomingPrefix.Length);
            _readHandler(this, data);
        }
        public void Read(RpcDataHandler handler)
        {
            _readHandler = handler;
        }
        public void ReadNonRpc(Action<object, string> handler)
        {
            _readNonRpcHandler = handler ?? DefaultOnReadNonRpc;
        }

        #endregion

        #region Write

        readonly object _writeGuard = new object();

        public void Write(string data)
        {
            if (!_opened)
                Debug.Print("rpc-stdio: output discarded (closed) - " + data);
            lock (_writeGuard)
                _output.WriteLine(SR.RpcIncomingPrefix + data);
        }

        #endregion
    }
}
