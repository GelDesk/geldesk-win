using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace GelDesk
{
    public class RpcConnection
    {
        public RpcConnection(IRpcChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            _channel = channel;

            _router = IoC.Get<RpcRouter>();

            channel.Read(channel_Read);
        }

        readonly IRpcChannel _channel;
        readonly IHandleRpc _router;

        #region Open, Close
        class States
        {
            public const int Closed = 0;
            public const int Opening = 1;
            public const int Opened = 2;
            public const int Closing = 3;
        }

        int _state;

        void OnOpenCompleted(Task task)
        {
            if (task != null &&
                task.Exception != null)
            {
                // TODO: Do something else with this error.
                Debug.Print("RpcConnection could not open due to error: {0}", task.Exception.Message);
                return;
            }
            if (Interlocked.CompareExchange(ref _state, States.Opened, States.Opening) != States.Opening)
                throw new InvalidOperationException("Invalid state (not opening).");
        }

        public Task Open()
        {
            if (Interlocked.CompareExchange(ref _state, States.Opening, States.Closed) >= States.Opening)
                throw new InvalidOperationException("Already opened or opening.");
            var openTask = _channel.Open();
            if (openTask != null)
                return openTask.ContinueWith(OnOpenCompleted);
            OnOpenCompleted(null);
            return null;
        }

        public void Close()
        {
            if (Interlocked.CompareExchange(ref _state, States.Closing, States.Opened) != States.Opened)
                return;
            _channel.Close();
            _state = States.Closed;
        }

        #endregion

        #region Notify
        public void Notify(string path, params Newtonsoft.Json.Linq.JToken[] arguments)
        {
            NotifyWithArguments(path, new Newtonsoft.Json.Linq.JArray(arguments));
        }
        public void Notify(Exception error)
        {
            var message = new RpcMessage();
            message.Error = error;
            Notify(message);
        }
        public void Notify(RpcMessage message)
        {
            message.IsNotification = true;
            SendCore(message);
        }
        public void NotifyWithArguments(string path, Newtonsoft.Json.Linq.JArray arguments)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            var message = new RpcMessage();
            message.Path = path;
            message.Arguments = arguments;
            Notify(message);
        }

        #endregion

        #region Receive
        void channel_Read(object channel, string data)
        {
            Debug.Print("rpc-in: " + data);
            RpcMessage message;
            Exception err;
            if (!RpcMessage.TryParse(data, out message, out err))
            {
                Notify(new RpcMessage()
                {
                    Error = err
                });
                return;
            }
            if (message.IsResponse)
                ReceiveResponse(message);
            else
                ReceiveRequestOrNotification(message);
        }
        void ReceiveRequestOrNotification(RpcMessage message)
        {
            var context = new RpcContext(this, message);
            try
            {
                if (_router.HandleRpc(context))
                    return;
            }
            catch (Exception ex)
            {
                context.Respond(ex);
                return;
            }
            context.MethodNotFound();
        }
        void ReceiveResponse(RpcMessage message)
        {
            TaskCompletionSource<RpcMessage> tsc;
            if (_sentPendingResponse.TryRemove(message.RequestId, out tsc))
            {
                try { tsc.SetResult(message); }
                catch (Exception ex)
                {
                    // TODO: Add a config option on whether to report this error to the server
                    // or replace this notification temporarily with error logging.
                    Notify(new RpcException(RpcErrorCodes.InternalError, ex));
                }
            }
            else {
                // TODO: Does the other side need to know? Because this should never happen 
                // unless there is a serious problem. So, we could just log it...
                // or make a config option for it.
                Notify(new RpcException(RpcErrorCodes.MethodNotFound));
            }
        }

        #endregion

        #region Request

        int _sentRequestCount;

        ConcurrentDictionary<int, TaskCompletionSource<RpcMessage>> _sentPendingResponse =
            new ConcurrentDictionary<int, TaskCompletionSource<RpcMessage>>();
        public Task<RpcMessage> Request(RpcMessage message)
        {
            // Validate
            if (!message.IsRequest)
                throw new InvalidOperationException("Message must be a request.");
            if (_state != States.Opened)
                throw new InvalidOperationException("Connection not open.");
            // Setup Response Delivery
            message.RequestId = System.Threading.Interlocked.Increment(ref _sentRequestCount);
            var tsc = new TaskCompletionSource<RpcMessage>(message);
            _sentPendingResponse.TryAdd(message.RequestId, tsc);
            // Send & Return
            SendCore(message);
            return tsc.Task;
        }
        public Task<RpcMessage> Request(string path, params Newtonsoft.Json.Linq.JToken[] arguments)
        {
            return RequestWithArguments(path, new Newtonsoft.Json.Linq.JArray(arguments));
        }
        public Task<RpcMessage> RequestWithArguments(string path, Newtonsoft.Json.Linq.JArray arguments)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            var message = new RpcMessage();
            message.Path = path;
            message.Arguments = arguments;
            return Request(message);
        }

        #endregion

        #region Respond
        public void Respond(RpcMessage message)
        {
            // Validate
            if (!message.IsResponse)
                throw new InvalidOperationException("Message is not a response.");
            if (_state != States.Opened)
                throw new InvalidOperationException("Connection not open.");
            SendCore(message);
        }

        #endregion

        #region Send
        void SendCore(RpcMessage message)
        {
            var data = message.Serialize();
            Debug.Print("rpc-out: " + data);
            _channel.Write(data);
        }

        #endregion
    }
}
