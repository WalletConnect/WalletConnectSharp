using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Tests.Models
{
    public class TopicData
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}