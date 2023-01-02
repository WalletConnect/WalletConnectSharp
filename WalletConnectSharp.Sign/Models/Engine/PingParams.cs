using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class PingParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}