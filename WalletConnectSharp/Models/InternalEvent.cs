using Newtonsoft.Json;

namespace WalletConnectSharp.Models
{
    public class InternalEvent
    {
        [JsonProperty("event")]
        public string @event;
    }
}