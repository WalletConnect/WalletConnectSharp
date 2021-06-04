using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Core.Events
{
    public class EventDelegator : IDisposable
    {
        
        private Dictionary<string, List<IEventProvider>> Listeners = new Dictionary<string, List<IEventProvider>>();

        public void ListenForResponse<T>(object id, EventHandler<GenericEvent<T>> callback)
        {
            ListenFor("response:" + id, callback);
        }
        
        
        public void ListenForResponse<T>(object id, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            ListenFor("response:" + id, callback);
        }

        public void ListenFor<T>(string eventId, EventHandler<GenericEvent<T>> callback)
        {
            //Console.WriteLine("Adding GenericEvent callback to event " + eventId);
            EventManager<T, GenericEvent<T>>.Instance.EventTriggers[eventId] += callback;
            
            //Console.WriteLine("Subscribing Event Provider to " + eventId);
            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }
        
        public void ListenFor<T>(string eventId, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            //Console.WriteLine("Adding JsonRpcResponse callback to event " + eventId);
            EventManager<T, JsonRpcResponseEvent<T>>.Instance.EventTriggers[eventId] += callback;

            //Console.WriteLine("Subscribing Event Provider to " + eventId);
            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }

        public void ListenFor<T>(string eventId, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            //Console.WriteLine("Adding JsonRpcRequest callback to event " + eventId);
            EventManager<T, JsonRpcRequestEvent<T>>.Instance.EventTriggers[eventId] += callback;
            
            //Console.WriteLine("Subscribing Event Provider to " + eventId);
            SubscribeProvider(eventId, EventFactory.Instance.ProviderFor<T>());
        }

        private void SubscribeProvider(string eventId, IEventProvider provider)
        {
            List<IEventProvider> listProvider;
            if (!Listeners.ContainsKey(eventId))
            {
                //Console.WriteLine("Adding new EventProvider list for " + eventId);
                listProvider = new List<IEventProvider>();
                Listeners.Add(eventId, listProvider);
            }
            else
            {
                listProvider = Listeners[eventId];
            }
            
            //Console.WriteLine("Adding listener to EventProvider list. New count: " + (listProvider.Count + 1));
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
                
                //Console.WriteLine("Triggering " + providerList.Count + " EventProviders for topic " + topic);

                for (int i = 0; i < providerList.Count; i++)
                {
                    var provider = providerList[i];
                    
                    provider.PropagateEvent(topic, json);
                }
            }
        }

        public void Dispose()
        {
            Listeners.Clear();
        }
    }
}