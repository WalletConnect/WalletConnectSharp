using Newtonsoft.Json;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth;

[RpcMethod("wc_authRequest")]
[RpcRequestOptions(Clock.ONE_DAY, 3000)]
[RpcResponseOptions(Clock.ONE_DAY, 3001)]
public class WcAuthRequest
{
    [JsonProperty("payloadParams")]
    public PayloadParams Payload;
    
    [JsonProperty("requester")]
    public Requester Requester { get; set; }
}
