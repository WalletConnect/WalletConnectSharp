using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class JsonRpcSubscriptionParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("data")]
        public SubscriptionData Data { get; set; }
    }
}