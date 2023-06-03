using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class TopicMessage : Message
{
    [JsonProperty("topic")]
    public string Topic { get; set; }
}
