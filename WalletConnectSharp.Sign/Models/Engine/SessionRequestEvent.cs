using Newtonsoft.Json;
using WalletConnectSharp.Network;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class SessionRequestEvent<T> : SessionEvent
    {
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("request")]
        public IRequestArguments<T> Request { get; set; }
    }
}