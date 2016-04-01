using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public class RpcRouter : IHandleRpc
    {
        public RpcRouter()
        {
            _handlers = new List<IHandleRpc>(1);
        }

        readonly List<IHandleRpc> _handlers;
        bool _routing;

        public void AddHandler(IHandleRpc handler)
        {
            if (_routing)
                throw new InvalidOperationException(
                    "Cannot add new handlers after routing has started.");
            _handlers.Add(handler);
        }

        public bool HandleRpc(RpcContext context)
        {
            var message = context.Message;
            Debug.Print("route: {0}", message.Path ?? message.RequestId.ToString());

            foreach (var handler in _handlers)
                if (handler.HandleRpc(context))
                    return true;
            return false;
        }

        public void Initialize()
        {
            if (_routing)
                throw new InvalidOperationException("Already initialized.");
            _routing = true;
        }
    }
}
