using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class UnsubscribeOptions : ProtocolOptionHolder
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}