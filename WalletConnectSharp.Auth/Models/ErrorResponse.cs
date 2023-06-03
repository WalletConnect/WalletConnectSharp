using Newtonsoft.Json;
using CommonErrorResponse = WalletConnectSharp.Network.Models.ErrorResponse;
namespace WalletConnectSharp.Auth.Models;

public class ErrorResponse : Message
{
    [JsonProperty("error")]
    public CommonErrorResponse Error { get; set; }
}
