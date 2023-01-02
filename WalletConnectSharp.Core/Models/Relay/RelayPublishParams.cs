using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class RelayPublishParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("ttl")]
        public long TTL { get; set; }
        
        [JsonProperty("prompt")]
        public bool Prompt { get; set; }
        
        [JsonProperty("tag")]
        public long? Tag { get; set; }
    }
}