using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Auth.Models;

public class PairingData : IKeyHolder<string>
{
    [JsonProperty("topic")]
    public string Topic { get; set; }
    
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
