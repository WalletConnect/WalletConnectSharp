using Newtonsoft.Json;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    public class SessionUpdateEvent : SessionEvent
    {
        [JsonProperty("params")]
        public SessionUpdate Params { get; set; }
    }
}
