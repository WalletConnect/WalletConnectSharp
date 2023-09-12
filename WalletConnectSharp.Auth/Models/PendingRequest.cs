using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class PendingRequest : Message
{
    [JsonProperty("pairingTopic")]
    public string PairingTopic;
    
    [JsonProperty("requester")]
    public Requester Requester { get; set; }
    
    [JsonProperty("cacaoPayload")]
    public Cacao.CacaoRequestPayload CacaoPayload { get; set; }
}
