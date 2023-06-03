using Newtonsoft.Json;
using CommonErrorResponse = WalletConnectSharp.Network.Models.ErrorResponse;

namespace WalletConnectSharp.Auth.Models;

public class AuthErrorResponse : TopicMessage
{
    [JsonProperty("params")]
    public CommonErrorResponse Error { get; set; }
}
