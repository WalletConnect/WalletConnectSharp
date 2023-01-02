using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class BaseRequiredNamespace
    {
        [JsonProperty("chains")]
        public string[] Chains { get; set; }
        
        [JsonProperty("methods")]
        public string[] Methods { get; set; }
        
        [JsonProperty("events")]
        public string[] Events { get; set; }
    }
}