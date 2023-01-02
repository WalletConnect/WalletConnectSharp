using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class ConnectParams
    {
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        [JsonProperty("pairingTopic")]
        public string PairingTopic { get; set; }
        
        [JsonProperty("relays")]
        public ProtocolOptions Relays { get; set; }
    }
}