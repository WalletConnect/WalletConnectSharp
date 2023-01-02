using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class PublishOptions : ProtocolOptionHolder
    {
        [JsonProperty("ttl")]
        public long TTL { get; set; }
        
        [JsonProperty("prompt")]
        public bool Prompt { get; set; }
        
        [JsonProperty("tag")]
        public long Tag { get; set; }
    }
}