using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class ApproveParams
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
        
        [JsonProperty("relayProtocol")]
        public string RelayProtocol { get; set; }
    }
}