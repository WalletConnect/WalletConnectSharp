using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class EmitParams<T>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("event")]
        public EventData<T> Event { get; set; }
    }
}