using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class Metadata
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
    
    [JsonProperty("icons")]
    public string[] Icons { get; set; }
    
    [JsonProperty("redirect")]
    public RedirectData Redirect { get; set; }
    
    [JsonProperty("verifyUrl")]
    public string VerifyUrl { get; set; }
}
