using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Models;

public class AuthResponse : TopicMessage
{
    [JsonProperty("params")]
    public JsonRpcResponse<Cacao> Response;
}
