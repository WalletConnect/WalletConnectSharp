using System;
using System.Collections.Generic;

namespace WalletConnectSharp.Events
{
    /// <summary>
    /// A class that simply holds the IEventProvider for a given event data type T. This is needed to keep the
    /// different event listeners (same eventId but different event data types) separate at runtime.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventFactory<T>
    {
        private static Dictionary<string, EventFactory<T>> _eventFactories = new Dictionary<string, EventFactory<T>>();
        private static readonly object _factoryLock = new object();
        private IEventProvider<T> _eventProvider;
        public string Context { get; private set; }

        private EventFactory(string context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Get the EventFactory for the event data type T
        /// </summary>
        public static EventFactory<T> InstanceOf(string context)
        {
            lock (_factoryLock)
            {
                if (!_eventFactories.ContainsKey(context))
                    _eventFactories.Add(context, new EventFactory<T>(context));

                return _eventFactories[context];
            }
        }
        
        /// <summary>
        /// Get the current EventProvider for the event data type T
        /// </summary>
        /// <exception cref="Exception">Internally only. When this value is set more than once</exception>
        public IEventProvider<T> Provider
        {
            get
            {
                return _eventProvider;
            }
            internal set
            {            
                if (_eventProvider != null)
                    throw new Exception("Provider for type " + typeof(T) + " already set");
                
                _eventProvider = value;
            }
        }
    }
}