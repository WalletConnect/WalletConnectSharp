using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class SubscriberParams : SubscribeOptions
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}