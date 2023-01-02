using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class JsonRpcUnsubscribeParams
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}