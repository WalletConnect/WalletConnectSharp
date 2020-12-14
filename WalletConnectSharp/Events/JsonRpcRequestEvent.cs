using System;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events
{
    public class JsonRpcRequestEvent<T> : EventArgs where T : JsonRpcRequest
    {
        public T Response { get; private set; }

        public JsonRpcRequestEvent(T response)
        {
            Response = response;
        }
    }
}