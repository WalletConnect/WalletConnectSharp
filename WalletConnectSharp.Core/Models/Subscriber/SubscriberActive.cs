using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class SubscriberActive : SubscriberParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}