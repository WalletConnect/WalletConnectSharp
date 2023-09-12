using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Models;

public class AuthErrorResponse : TopicMessage
{
    [JsonProperty("params")]
    public Error Error;
}
