using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Auth.Models;

public class AuthData : IKeyHolder<string>
{
    [JsonProperty("responseTopic")]
    public string ResponseTopic { get; set; }
    
    [JsonProperty("publicKey")]
    public string PublicKey { get; set; }

    public string Key
    {
        get
        {
            return ResponseTopic;
        }
    }
}
