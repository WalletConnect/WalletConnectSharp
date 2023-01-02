using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class RelayerClientMetadata
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        
        [JsonProperty("version")]
        public int Version { get; set; }
        
        [JsonProperty("env")]
        public string Env { get; set; }
        
        [JsonProperty("host")]
        public string Host { get; set; }
    }
}