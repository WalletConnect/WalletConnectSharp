using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class DisconnectParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("reason")]
        public ErrorResponse Reason { get; set; }
    }
}