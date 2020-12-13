using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events
{
    public class EventManager
    {
        public event EventHandler<JsonRpcResponseEvent<WCSessionRequestResponse>> SessionResponse;

        private Dictionary<Type, Action<JsonRpcResponse>> lookup = new Dictionary<Type, Action<JsonRpcResponse>>();

        public EventManager()
        {
            var emType = typeof(EventManager);
            var allEvents = emType.GetEvents();

            foreach (var eventInfo in allEvents)
            {
                EventInfo @event = eventInfo;
                FieldInfo eventField = emType.GetField(@event.Name, BindingFlags.NonPublic |
                                                                    BindingFlags.Instance);
                var eventResponseType = eventInfo.EventHandlerType.GetGenericArguments()[0];
                var key = eventResponseType.GetGenericArguments()[0];
                var action = new Action<JsonRpcResponse>(delegate(JsonRpcResponse response)
                {
                    var eventArgs = Activator.CreateInstance(eventResponseType, response);
                    
                    var @delegate = (System.Delegate) eventField.GetValue(this);

                    foreach (var subscriber in @delegate.GetInvocationList())
                    {
                        subscriber.DynamicInvoke(this, eventArgs);
                    }
                });
                
                lookup.Add(key, action);
            }
        }

        public void ExecuteEvent<T>(T value) where T : JsonRpcResponse
        {
            var vtype = typeof(T);

            if (lookup.ContainsKey(vtype))
            {
                lookup[vtype].Invoke(value);
            }
        }
    }
}