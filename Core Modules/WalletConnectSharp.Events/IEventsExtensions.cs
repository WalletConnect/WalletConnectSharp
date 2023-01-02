using System;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Events.Model;

namespace WalletConnectSharp.Events
{
    public static class IEventsExtensions
    {
        public static void On(this IEvents eventEmitter, string eventId, Action callback)
        {
            eventEmitter.On<object>(eventId, (sender, @event) =>
            {
                callback();
            });
        }
        
        public static void On<T>(this IEvents eventEmitter, string eventId, EventHandler<GenericEvent<T>> callback)
        {
            eventEmitter.Events.ListenFor(eventId, callback);
        }

        public static void Once<T>(this IEvents eventEmitter, string eventId, EventHandler<GenericEvent<T>> callback)
        {
            eventEmitter.Events.ListenForOnce(eventId, callback);
        }

        public static void Off<T>(this IEvents eventEmitter, string eventId, EventHandler<GenericEvent<T>> callback)
        {
            eventEmitter.Events.RemoveListener(eventId, callback);
        }

        public static void RemoveListener<T>(this IEvents eventEmitter, string eventId, EventHandler<GenericEvent<T>> callback)
        {
            eventEmitter.Events.RemoveListener(eventId, callback);
        }
    }
}