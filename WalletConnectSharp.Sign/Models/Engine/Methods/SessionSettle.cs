using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionSettle")]
    [RpcRequestOptions(Clock.FIVE_MINUTES, false, 1102)]
    [RpcResponseOptions(Clock.FIVE_MINUTES, false, 1103)]
    public class SessionSettle : IWcMethod
    {
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
        
        [JsonProperty("expiry")]
        public long Expiry { get; set; }
        
        [JsonProperty("controller")]
        public Participant Controller { get; set; }
    }
}