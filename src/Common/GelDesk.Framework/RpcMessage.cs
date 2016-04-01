using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public sealed class RpcMessage
    {
        #region Constructor
        public RpcMessage() { }

        #endregion

        #region Message Elements
        public string Path { get; set; }
        public JArray Arguments { get; set; }
        public int RequestId { get; set; }

        #endregion

        #region Message Type
        public Exception Error { get; set; }
        public bool HasError { get { return Error != null; } }
        public bool IsNotification
        {
            get { return Path != null && RequestId < 0; }
            set { RequestId = value ? -1 : 0; }
        }
        public bool IsRequest { get { return Path != null && RequestId > -1; } }
        public bool IsResponse { get { return Path == null; } }

        #endregion

        #region Parsing & Serializing

        // Request Frame: ["path", [data...], request-id] 
        // Response Frame: [[error/error-code/null, undefined/data...], request-id]

        public static bool TryParse(string frame, out RpcMessage message, out Exception err)
        {
            if (frame == null)
            {
                message = null;
                err = new RpcException(RpcErrorCodes.ParseError, "null message.");
                return false;
            }
            try
            {
                var fa = JArray.Parse(frame);
                var msg = new RpcMessage();

                if (fa.Count > 0)
                    msg.RequestId = (int)fa.Last;
                if (fa.Count > 1)
                    msg.Arguments = (JArray)fa[fa.Count - 2];
                if (fa.Count > 2)
                    msg.Path = (string)fa[fa.Count - 3];

                ExtractErrorFromData(msg);

                message = msg;
                err = null;
                return true;
            } catch(Exception ex)
            {
                message = null;
                err = new RpcException(RpcErrorCodes.ParseError, ex);
            }
            return false;
        }

        static bool ExtractErrorFromData(RpcMessage message)
        {
            if (!message.IsResponse 
                || message.Arguments == null 
                || message.Arguments.Count < 1)
                return false;
            var token = message.Arguments[0];
            message.Arguments.RemoveAt(0);
            if (token.Type == JTokenType.Integer)
                message.Error = new RpcException((int)token);
            else if (token.Type == JTokenType.Object)
                message.Error = RpcException.FromJObject((JObject)token);
            else
                return false;
            return true;
        }

        public string Serialize()
        {
            var fa = new JArray();
            // Path?
            if (!IsResponse)
                fa.Add(Path);
            // Arguments...
            JArray args = Arguments != null ? 
                new JArray(Arguments) : 
                new JArray();
            if (!IsRequest)
            {
                if (!HasError)
                {
                    if (!IsNotification)
                        args.Insert(0, null);
                }
                else
                {
                    // Error Data
                    var rpcErr = Error as RpcException;
                    if (rpcErr != null && rpcErr.Code != 0)
                        args.Insert(0, rpcErr.Code);
                    else
                    {
                        var err = new JObject();
                        err["message"] = Error.Message;
                        err["stack"] = Error.StackTrace;
                        args.Insert(0, err);
                    }
                }
            }
            // Arguments
            fa.Add(args);
            // RequestId
            fa.Add(RequestId);
            // Done
            return fa.ToString(Formatting.None);
        }

        #endregion
    }
}
