using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public abstract class ProtocolOptionHolder
    {
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
    }
}