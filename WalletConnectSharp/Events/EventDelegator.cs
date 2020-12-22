using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WalletConnectSharp.Events.Request;
using WalletConnectSharp.Events.Response;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events
{
    public class EventDelegator : IDisposable
    {
        
        private Dictionary<string, List<IEventProvider>> Listeners = new Dictionary<string, List<IEventProvider>>();

        public void ListenForResponse<T>(object id, EventHandler<GenericEventArgs<T>> callback)
        {
            ListenFor("response:" + id, callback);
        }
        
        
        public void ListenForResponse<T>(object id, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            ListenFor("response:" + id, callback);
        }

        public void ListenFor<T>(string eventId, EventHandler<GenericEventArgs<T>> callback)
        {
            EventManager<T>.Instance.EventTrigger += callback;
            
            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }
        
        public void ListenFor<T>(string eventId, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            JsonRpcResponseEventManager<T>.Instance.EventTrigger += callback;

            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }

        public void ListenFor<T>(string eventId, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            JsonRpcRequestEventManager<T>.Instance.EventTrigger += callback;
            
            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }

        private void SubscribeProvider(string eventId, IEventProvider provider)
        {
            List<IEventProvider> listProvider;
            if (!Listeners.ContainsKey(eventId))
            {
                listProvider = new List<IEventProvider>();
                Listeners.Add(eventId, listProvider);
            }
            else
            {
                listProvider = Listeners[eventId];
            }
            
            listProvider.Add(provider);
        }
        
        public void Trigger<T>(string topic, T obj)
        {
            Trigger(topic, JsonConvert.SerializeObject(obj));
        }


        public void Trigger(string topic, string json)
        {
            if (Listeners.ContainsKey(topic))
            {
                var providerList = Listeners[topic];

                foreach (var provider in providerList)
                {
                    provider.PropagateEvent(json);
                }
            }
        }

        public void Dispose()
        {
            Listeners.Clear();
        }
    }
}