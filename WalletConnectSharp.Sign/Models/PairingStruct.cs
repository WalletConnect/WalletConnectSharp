using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models
{
    public struct PairingStruct : IKeyHolder<string>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonIgnore]
        public string Key
        {
            get
            {
                return Topic;
            }
        }
        
        [JsonProperty("expiry")]
        public long? Expiry { get; set; }
        
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        [JsonProperty("active")]
        public bool? Active { get; set; }
        
        [JsonProperty("peerMetadata")]
        public Metadata PeerMetadata { get; set; }
    }
}