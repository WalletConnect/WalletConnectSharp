using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class AuthPayload
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public Cacao.CacaoHeader? Type { get; set; }
    
    [JsonProperty("chainId")]
    public string ChainId { get; set; }

    [JsonProperty("domain")]
    public string Domain { get; set; }

    [JsonProperty("aud")]
    public string Aud { get; set; }
    
    [JsonProperty("nonce")]
    public string Nonce { get; set; }
    
    [JsonProperty("nbf", NullValueHandling = NullValueHandling.Ignore)]
    public string Nbf { get; set; }
    
    [JsonProperty("exp", NullValueHandling = NullValueHandling.Ignore)]
    public string Exp { get; set; }
    
    [JsonProperty("statement", NullValueHandling = NullValueHandling.Ignore)]
    public string Statement { get; set; }
    
    [JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
    public string RequestId { get; set; }
    
    [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Resources { get; set; }
}
