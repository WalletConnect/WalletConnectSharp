using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    [RpcResponseOptions(Clock.FIVE_MINUTES, false, 1101)]
    public class SessionProposeResponse
    {
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        [JsonProperty("responderPublicKey")]
        public string ResponderPublicKey { get; set; }
    }
}
