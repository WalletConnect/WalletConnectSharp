using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class IridiumJWTPayload
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }
        
        [JsonProperty("sub")]
        public string Sub { get; set; }
        
        [JsonProperty("aud")]
        public string Aud { get; set; }
        
        [JsonProperty("iat")]
        public long Iat { get; set; }
        
        [JsonProperty("exp")]
        public long Exp { get; set; }
    }
}