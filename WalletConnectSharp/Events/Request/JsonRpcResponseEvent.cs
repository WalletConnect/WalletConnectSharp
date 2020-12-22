using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events.Request
{
    public class JsonRpcResponseEvent<T> : GenericEventArgs<T> where T : JsonRpcResponse
    {
        public JsonRpcResponseEvent(T response) : base(response)
        {
        }
    }
}