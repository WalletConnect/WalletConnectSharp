using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class RejectParams
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("reason")]
        public ErrorResponse Reason { get; set; }
    }
}