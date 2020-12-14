using Nethereum.WalletConnect.Models;

namespace Nethereum.WalletConnect.Events.Response
{
    public class JsonRpcRequestEvent<T> : GenericEventArgs<T> where T : JsonRpcRequest
    {
        public JsonRpcRequestEvent(T response) : base(response)
        {
        }
    }
}