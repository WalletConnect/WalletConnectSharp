using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WalletConnectSharp.Models
{
    public abstract class JsonRpcRequest : IEventSource
    {
        [JsonProperty]
        private int id;
        [JsonProperty]
        private string jsonrpc = "2.0";
        
        [JsonProperty("method")]
        public abstract string Method { get; }
        
        public int ID
        {
            get { return id; }
        }

        public string JsonRPC
        {
            get { return jsonrpc; }
        }

        public string Event
        {
            get { return Method; }
        }
    }
}