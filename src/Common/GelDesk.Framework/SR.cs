using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    static class SR
    {
        public const string DefaultConfigFileName = "geldesk.json";

        public const string RpcPathSeparator = "/";
        public const char RpcPathSeparatorChar = '/';

        public const string RpcIncomingPrefix = "@geldesk:";
        
        public const string RpcParseError = "Parse error";
        public const string RpcInvalidRequestError = "Invalid Request";
        public const string RpcInternalError = "Internal error";
        public const string RpcMethodNotFoundError = "Method not found";
        public const string RpcInvalidArgsError = "Invalid arguments";

        public static string RpcErrorMessage(int code)
        {
            switch (code)
            {
                case RpcErrorCodes.ParseError:
                    return RpcParseError;
                case RpcErrorCodes.InvalidRequest:
                    return RpcInvalidRequestError;
                case RpcErrorCodes.InternalError:
                    return RpcInternalError;
                case RpcErrorCodes.MethodNotFound:
                    return RpcMethodNotFoundError;
                case RpcErrorCodes.InvalidArgs:
                    return RpcInvalidArgsError;
            }
            return RpcInternalError;
        }
    }
}
