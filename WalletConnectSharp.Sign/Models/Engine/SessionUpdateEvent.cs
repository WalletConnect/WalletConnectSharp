using Newtonsoft.Json;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class SessionUpdateEvent : SessionEvent
    {
        [JsonProperty("params")]
        public SessionUpdate Params { get; set; }
    }
}