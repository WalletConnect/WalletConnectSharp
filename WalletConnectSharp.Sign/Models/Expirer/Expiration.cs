using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Expirer
{
    public class Expiration
    {
        [JsonProperty("target")]
        public string Target { get; set; }
        
        [JsonProperty("expiry")]
        public long Expiry { get; set; }
    }
}