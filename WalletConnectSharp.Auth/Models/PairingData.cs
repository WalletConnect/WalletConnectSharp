using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Auth.Models;

public class PairingData : IKeyHolder<string>
{
    [JsonProperty("topic")]
    public string Topic;
    
    [JsonProperty("pairingTopic")]
    public string PairingTopic { get; set; }

    public string Key
    {
        get
        {
            return PairingTopic;
        }
    }
}
