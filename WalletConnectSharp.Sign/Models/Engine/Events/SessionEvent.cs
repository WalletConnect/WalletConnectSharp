using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    public class SessionEvent
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}
