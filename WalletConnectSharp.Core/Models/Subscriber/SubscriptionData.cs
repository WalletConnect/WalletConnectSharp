using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class SubscriptionData
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}