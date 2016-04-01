using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public sealed class RpcContext
    {
        public RpcContext(RpcConnection connection, RpcMessage message)
        {
            _connection = connection;
            _message = message;
            _fullPath = InitPath(message.Path, out _targetPath, out _actionName);
        }

        readonly RpcConnection _connection;
        readonly RpcMessage _message;
        public RpcConnection Connection { get { return _connection; } }
        public RpcMessage Message { get { return _message; } }
        public EventPublisher EventPublisher { get; set; }

        #region Path

        readonly string _fullPath;
        readonly string _actionName;
        readonly string _targetPath;

        public string ActionName { get { return _actionName; } }
        public string FullPath { get { return _fullPath; } }
        public string TargetPath { get { return _targetPath; } }

        string InitPath(string path, out string objectPath, out string actionName)
        {
            var i = path.LastIndexOf(SR.RpcPathSeparatorChar);
            if (i < 0)
            {
                actionName = path;
                objectPath = "";
            }
            else
            {
                actionName = path.Substring(i + 1);
                objectPath = path.Substring(0, i);
            }
            return path;
        }
        #endregion

        #region Responding
        public void MethodNotFound()
        {
            if (Message.IsRequest)
                Respond(new RpcException(RpcErrorCodes.MethodNotFound));
            else
                Connection.Notify(new RpcException(RpcErrorCodes.MethodNotFound));
        }
        public void Respond(params JToken[] arguments)
        {
            RespondWithArguments(null, new JArray(arguments));
        }
        public void Respond(Exception error)
        {
            RespondWithArguments(error, null);
        }
        public void Respond(Exception error, params JToken[] arguments)
        {
            RespondWithArguments(error, new JArray(arguments));
        }
        public void RespondWithArguments(JArray arguments)
        {
            RespondWithArguments(null, arguments);
        }
        public void RespondWithArguments(Exception error, JArray arguments = null)
        {
            // Silently discard responses to notifications (aka: requests that 
            // didn't ask for a callback).
            if (Message.IsNotification)
                return;
            // Validate that this request is respondable.
            if (!Message.IsRequest)
                throw new InvalidOperationException("Cannon respond to non-response message.");
            else if (Message.RequestId < 0)
                throw new InvalidOperationException("Cannon respond to un-sent request.");
            // Send response.
            var response = new RpcMessage();
            response.Error = error;
            response.Arguments = arguments;
            response.RequestId = Message.RequestId;
            Connection.Respond(response);
        }
        #endregion
    }
}
