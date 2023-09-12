using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class AuthPayload
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string? Type;
    
    [JsonProperty("chainId")]
    public string ChainId;

    [JsonProperty("domain")]
    public string Domain;

    [JsonProperty("aud")]
    public string Aud;
    
    [JsonProperty("nonce")]
    public string Nonce;
    
    [JsonProperty("nbf", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbf;
    
    [JsonProperty("exp", NullValueHandling = NullValueHandling.Ignore)]
    public string Exp;
    
    [JsonProperty("statement", NullValueHandling = NullValueHandling.Ignore)]
    public string Statement;
    
    [JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
    public string RequestId;
    
    [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Resources;
}
