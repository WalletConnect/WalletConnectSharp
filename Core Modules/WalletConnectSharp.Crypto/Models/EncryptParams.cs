using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class EncryptParams
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("symKey")]
        public string SymKey { get; set; }
        
        [JsonProperty("type")]
        public int Type { get; set; }
        
        [JsonProperty("iv")]
        public string Iv { get; set; }
        
        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey { get; set; }
    }
}