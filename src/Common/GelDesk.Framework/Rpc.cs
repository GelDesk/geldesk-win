using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public delegate void RpcDataHandler(object channel, string data);
    public delegate void RpcHandler(RpcContext rpc);

    public interface IRpcChannel
    {
        Task Open();
        void Close();
        void Read(RpcDataHandler handler);
        void Write(string data);
    }
    public interface IHandleRpc
    {
        bool HandleRpc(RpcContext context);
    }

    public static class RpcErrorCodes
    {
        public const int ReservedBegin = -32000;
        public const int ReservedEnd = -32099;

        public const int ParseError = -32700;

        public const int InvalidRequest = -32600;
        public const int InternalError = InvalidRequest - 1;
        public const int MethodNotFound = InvalidRequest - 2;
        public const int InvalidArgs = InvalidRequest - 3;
    }
    public class RpcException : Exception
    {
        #region Constructor
        public RpcException()
            : this(0, null, null)
        { }
        public RpcException(int code)
            : this(code, null, null)
        { }
        public RpcException(int code, Exception innerException)
            : this(code, null, innerException)
        { }
        public RpcException(int code, string message)
            : this(code, message, null)
        { }
        public RpcException(int code, string message, Exception innerException)
            : base(message ?? SR.RpcErrorMessage(code), innerException)
        {
            this.Code = code;
        }
        public RpcException(string message)
            : this(0, message, null)
        { }
        public RpcException(string message, Exception innerException)
            : this(0, message, innerException)
        { }

        #endregion
        public int Code { get; private set; }
        public string Stack { get; private set; }

        public static RpcException FromJObject(JObject errorObject)
        {
            var token = errorObject["code"];
            var code = 0;
            if (token != null && token.Type == JTokenType.Integer)
                code = (int)token;
            var msgStr = (string)errorObject["message"];
            var stackStr = (string)errorObject["stack"];
            var err = new RpcException(code, msgStr);
            if (stackStr != null)
                err.Stack = stackStr;
            return err;
        }
    }

    public static class RpcPath
    {
        public static string Join(string string1, string string2)
        {
            if (string1 != null && string2 != null)
                return string1 + SR.RpcPathSeparator + string2;
            return string1 ?? string2;
        }
        public static string[] Split(string path)
        {
            return path.SplitWith(SR.RpcPathSeparatorChar);
        }
    }

    public static class RpcSerializerConfig
    {
        public static JsonSerializerSettings CreateDefaultJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public static void ApplyGlobalDefaultSerializerSettings()
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = CreateDefaultJsonSerializerSettings;
            // ... other serializers that may be used ...
        }
    }
}
