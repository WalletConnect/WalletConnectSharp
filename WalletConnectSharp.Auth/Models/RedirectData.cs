using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class RedirectData
{
    [JsonProperty("native")]
    public string Native { get; set; }
    
    [JsonProperty("universal")]
    public string Universal { get; set; }
}
