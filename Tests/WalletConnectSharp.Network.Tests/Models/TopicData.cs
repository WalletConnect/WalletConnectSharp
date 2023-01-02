using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Tests.Models
{
    public class TopicData
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}