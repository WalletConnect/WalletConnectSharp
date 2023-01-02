using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class MessageEvent
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}