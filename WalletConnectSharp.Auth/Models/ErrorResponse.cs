using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Models;

public class ErrorResponse : Message
{
    [JsonProperty("error")]
    public Error Error;
}
