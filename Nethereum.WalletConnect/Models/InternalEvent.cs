using Newtonsoft.Json;

namespace Nethereum.WalletConnect.Models
{
    public class InternalEvent
    {
        [JsonProperty("event")]
        public string @event;
    }
}