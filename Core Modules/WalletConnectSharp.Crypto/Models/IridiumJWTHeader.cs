using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class IridiumJWTHeader
    {
        public static readonly IridiumJWTHeader DEFAULT = new IridiumJWTHeader()
        {
            Alg = "EdDSA",
            Typ = "JWT"
        };
        
        [JsonProperty("alg")]
        public string Alg { get; set; }
        
        [JsonProperty("typ")]
        public string Typ { get; set; }
    }
}