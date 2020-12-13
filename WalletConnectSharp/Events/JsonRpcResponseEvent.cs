using System;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events
{
    public class JsonRpcResponseEvent<T> : EventArgs where T : JsonRpcResponse
    {
        public T Response { get; private set; }

        public JsonRpcResponseEvent(T response)
        {
            Response = response;
        }

        public static Action CreateAction()
        {
            return () =>
            {
                
            };
        }
    }
}