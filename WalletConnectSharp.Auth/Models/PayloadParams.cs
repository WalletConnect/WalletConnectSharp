using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class PayloadParams : AuthPayload
{
    [JsonProperty("version")]
    public string Version { get; set; }
    
    [JsonProperty("iat")]
    public string Iat { get; set; }
}
