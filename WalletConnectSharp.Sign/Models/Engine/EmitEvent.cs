using Newtonsoft.Json;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class EmitEvent<T> : SessionEvent
    {
        [JsonProperty("params")]
        public SessionEvent<T> Params { get; set; }
    }
}