using System;
using Newtonsoft.Json;

namespace WalletConnectSharp.Events
{
    public class EventManager<T> : IEventProvider
    {
        private static EventManager<T> _instance;
        
        public event EventHandler<GenericEventArgs<T>> EventTrigger;

        public static EventManager<T> Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new EventManager<T>();
                }
                
                return _instance; 
            }
        }

        //private Dictionary<string, Type> MethodLookup = new Dictionary<string, Type>();

        //private Dictionary<Type, Action<JsonRpcResponse>> lookup = new Dictionary<Type, Action<JsonRpcResponse>>();

        private EventManager()
        {
            EventFactory.Instance.Register<T>(this);
        }

        public void PropagateEvent(string responseJson)
        {
            if (EventTrigger != null)
            {
                var response = JsonConvert.DeserializeObject<T>(responseJson);
                
                EventTrigger(this, new GenericEventArgs<T>(response));
            }
        }
    }
}