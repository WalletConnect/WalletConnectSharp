using Newtonsoft.Json;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class RequestParams<T>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("request")]
        public JsonRpcRequest<T> Request { get; set; }
    }
}