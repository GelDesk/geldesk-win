using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SubscribersList = System.Collections.Generic.HashSet<GelDesk.RpcConnection>;
using SubscriptionsList = System.Collections.Generic.Dictionary<string,
    System.Collections.Generic.HashSet<GelDesk.RpcConnection>>;

namespace GelDesk
{
    public sealed class EventPublisher
    {
        public EventPublisher()
        {
            _subscriptions = new SubscriptionsList(7);
        }
        
        readonly SubscriptionsList _subscriptions;

        public void Publish(string eventPath, params JToken[] arguments)
        {
            PublishWithArguments(eventPath, new JArray(arguments));
        }
        public void PublishWithArguments(string eventPath, JArray arguments)
        {
            SubscribersList subs;
            if (!_subscriptions.TryGetValue(eventPath, out subs))
                return;
            var subsArray = subs.ToArray();
            foreach (var sub in subsArray)
                sub.NotifyWithArguments(eventPath, arguments);
        }
        public void Subscribe(string eventPath, RpcConnection subscriber)
        {
            SubscribersList subs;
            if (!_subscriptions.TryGetValue(eventPath, out subs))
            {
                subs = new SubscribersList();
                _subscriptions.Add(eventPath, subs);
            }
            subs.Add(subscriber);
        }
        public void Unsubscribe(string eventPath, RpcContext rpc)
        {
            Unsubscribe(eventPath, rpc.Connection);
        }
        public void Unsubscribe(string eventPath, RpcConnection subscriber)
        {
            SubscribersList subs;
            if (!_subscriptions.TryGetValue(eventPath, out subs))
                return;
            subs.Remove(subscriber);
        }
        public void UnsubscribeAll(string eventPath)
        {
            _subscriptions.Remove(eventPath);
        }
        public void UnsubscribeAll(RpcConnection connection)
        {
            foreach (var kvp in _subscriptions)
                kvp.Value.Remove(connection);
        }
    }

    public static class EventPublisherExtensions
    {
        public static void Subscribe(this EventPublisher publisher, string eventPath, RpcContext rpc)
        {
            publisher.Subscribe(eventPath, rpc.Connection);
        }
        public static void UnsubscribeAll(this EventPublisher publisher, RpcContext rpc)
        {
            publisher.UnsubscribeAll(rpc.Connection);
        }
    }
}
