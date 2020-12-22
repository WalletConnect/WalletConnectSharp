using WalletConnectSharp.Models;

namespace WalletConnectSharp.Events.Response
{
    public class JsonRpcRequestEvent<T> : GenericEventArgs<T> where T : JsonRpcRequest
    {
        public JsonRpcRequestEvent(T response) : base(response)
        {
        }
    }
}