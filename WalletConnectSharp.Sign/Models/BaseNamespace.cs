using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class BaseNamespace
    {
        [JsonProperty("accounts")]
        public string[] Accounts { get; set; }
        
        [JsonProperty("methods")]
        public string[] Methods { get; set; }
        
        [JsonProperty("events")]
        public string[] Events { get; set; }
    }
}