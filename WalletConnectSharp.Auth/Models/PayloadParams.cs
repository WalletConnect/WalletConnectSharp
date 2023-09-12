using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class PayloadParams : AuthPayload
{
    [JsonProperty("version")]
    public string Version;
    
    [JsonProperty("iat")]
    public string Iat { get; set; }
}
