using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Expirer
{
    public class ExpirerEventArgs
    {
        [JsonProperty("target")]
        public string Target { get; set; }
        
        [JsonProperty("expiration")]
        public Expiration Expiration { get; set; }
    }
}