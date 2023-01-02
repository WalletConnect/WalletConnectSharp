using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class JsonRpcSubscriberParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}