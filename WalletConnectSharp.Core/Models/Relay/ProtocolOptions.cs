using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class ProtocolOptions
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}