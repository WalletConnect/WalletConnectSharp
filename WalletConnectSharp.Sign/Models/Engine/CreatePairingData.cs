using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class CreatePairingData
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}