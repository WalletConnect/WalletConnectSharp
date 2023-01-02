using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class EncodingParams
    {
        [JsonProperty("type")]
        public byte[] Type { get; set; }
        
        [JsonProperty("sealed")]
        public byte[] Sealed { get; set; }
        
        [JsonProperty("iv")]
        public byte[] Iv { get; set; }
        
        [JsonProperty("senderPublicKey")]
        public byte[] SenderPublicKey { get; set; }
    }
}