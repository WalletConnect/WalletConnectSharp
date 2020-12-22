using System;
using Newtonsoft.Json;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events.Request
{
    public class JsonRpcResponseEventManager<T> : IEventProvider where T : JsonRpcResponse
    {
        private static JsonRpcResponseEventManager<T> _instance;
        
        public event EventHandler<JsonRpcResponseEvent<T>> EventTrigger;

        public static JsonRpcResponseEventManager<T> Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new JsonRpcResponseEventManager<T>();
                }
                
                return _instance; 
            }
        }

        //private Dictionary<string, Type> MethodLookup = new Dictionary<string, Type>();

        //private Dictionary<Type, Action<JsonRpcResponse>> lookup = new Dictionary<Type, Action<JsonRpcResponse>>();

        private JsonRpcResponseEventManager()
        {
            EventFactory.Instance.Register<T>(this);
        }

        public void PropagateEvent(string responseJson)
        {
            if (EventTrigger != null)
            {
                var response = JsonConvert.DeserializeObject<T>(responseJson);
                
                EventTrigger(this, new JsonRpcResponseEvent<T>(response));
            }
        }
    }
}