using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class RespondParams<T>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("response")]
        public JsonRpcResponse<T> Response { get; set; }
    }
}