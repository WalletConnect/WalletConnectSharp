using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class SessionProposeResponse
    {
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        [JsonProperty("responderPublicKey")]
        public string ResponderPublicKey { get; set; }
    }
}