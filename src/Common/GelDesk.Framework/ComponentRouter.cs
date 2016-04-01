using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace GelDesk
{
    public class ComponentRouter : IHandleRpc
    {
        public ComponentRouter(ComponentsCollection rootComponents)
        {
            _rootComponents = rootComponents;
        }

        readonly ComponentsCollection _rootComponents;

        public bool HandleRpc(RpcContext context)
        {
            // For now, all "components" exist on the UI thread. Therefore, 
            // we just marshall the call there, where we don't need to worry 
            // about multiple threads operating on a single collection.

            DirectlyExecute.BeginOnUIThread(RouteMessageUI, context);
            
            // Since we're searching for the final handler on the UI thread, 
            // we must return true here and the UI thread must handle the 
            // automatic error response/notification if something goes wrong.
            //
            // For this reason, ComponentRouter should be the last handler 
            // executed by the main RpcRouter.

            return true;
        }

        void RouteMessageUI(object rpc)
        {
            var context = (RpcContext)rpc;
            var message = context.Message;
            try {
                Debug.Print("route-ui: {0}", message.Path ?? message.RequestId.ToString());
                // Find component and let it handle the message.
                ComponentObject component;
                if (_rootComponents.TryGetByPath(context.TargetPath, out component))
                {
                    if (component.HandleRpc(context))
                        return;
                }
            } catch(Exception ex)
            {
                context.Respond(ex);
                return;
            }
            context.MethodNotFound();
        }
    }
}