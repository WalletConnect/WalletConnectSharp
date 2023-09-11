using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class RequestUri
{
    [JsonProperty("id")]
    public long Id;
    
    [JsonProperty("uri")]
    public string Uri { get; set; }
}
