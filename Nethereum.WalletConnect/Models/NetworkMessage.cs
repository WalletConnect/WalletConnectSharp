using Newtonsoft.Json;

namespace Nethereum.WalletConnect.Models
{
    public class NetworkMessage
    {
        [JsonProperty("topic")]
        public string Topic;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("payload")]
        public string Payload;

        [JsonProperty("silent")]
        public bool Silent;
    }
}