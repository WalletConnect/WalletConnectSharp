using Newtonsoft.Json;
using WalletConnectSharp.Events.Utils;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Represents a full JSON RPC request with the given parameter type T
    /// </summary>
    /// <typeparam name="T">The parameter type for this JSON RPC request</typeparam>
    public class JsonRpcRequest<T> : IJsonRpcRequest<T>
    {
        public string Method { get; set; }
        public T Params { get; set; }
        public long Id { get; set; }

        public string JsonRPC
        {
            get
            {
                return "2.0";
            }
        }

        public JsonRpcRequest()
        {
        }

        public JsonRpcRequest(string method, T param, long? id = null)
        {
            if (id == null)
            {
                id = RpcPayloadId.Generate();
            }

            this.Method = method;
            this.Params = param;
            this.Id = (long)id;
        }
    }
}