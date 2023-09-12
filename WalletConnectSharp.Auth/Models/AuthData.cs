using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Auth.Models;

public class AuthData : IKeyHolder<string>
{
    [JsonProperty("responseTopic")]
    public string ResponseTopic;
    
    [JsonProperty("publicKey")]
    public string PublicKey;

    public string Key
    {
        get
        {
            return ResponseTopic;
        }
    }
}
