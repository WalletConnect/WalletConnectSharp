using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class UriParameters
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        
        [JsonProperty("version")]
        public int Version { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("symKey")]
        public string SymKey { get; set; }
        
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
    }
}