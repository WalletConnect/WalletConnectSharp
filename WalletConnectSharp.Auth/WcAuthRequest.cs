using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Models.Engine;

[RpcMethod("wc_authRequest")]
[RpcRequestOptions(Clock.ONE_DAY, 3000)]
[RpcResponseOptions(Clock.ONE_DAY, 3001)]
public class WcAuthRequest
{
    [JsonProperty("payloadParams")]
    public PayloadParams Payload { get; set; }
    
    [JsonProperty("requester")]
    public Requester Requester { get; set; }
}
