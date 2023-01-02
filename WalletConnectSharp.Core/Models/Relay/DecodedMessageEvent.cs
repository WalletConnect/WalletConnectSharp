using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class DecodedMessageEvent
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("payload")]
        public JsonRpcPayload Payload { get; set; }
    }
}