using System;
using Newtonsoft.Json;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events.Response
{
    public class JsonRpcRequestEventManager<T> : IEventProvider where T : JsonRpcRequest
    {
        private static JsonRpcRequestEventManager<T> _instance;
        
        public event EventHandler<JsonRpcRequestEvent<T>> EventTrigger;

        public static JsonRpcRequestEventManager<T> Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new JsonRpcRequestEventManager<T>();
                }
                
                return _instance; 
            }
        }

        //private Dictionary<string, Type> MethodLookup = new Dictionary<string, Type>();

        //private Dictionary<Type, Action<JsonRpcResponse>> lookup = new Dictionary<Type, Action<JsonRpcResponse>>();

        private JsonRpcRequestEventManager()
        {
            EventFactory.Instance.Register<T>(this);
        }

        public void PropagateEvent(string responseJson)
        {
            if (EventTrigger != null)
            {
                var response = JsonConvert.DeserializeObject<T>(responseJson);
                
                EventTrigger(this, new JsonRpcRequestEvent<T>(response));
            }
        }
    }
}