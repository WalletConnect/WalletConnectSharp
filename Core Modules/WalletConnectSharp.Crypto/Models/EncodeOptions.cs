using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class EncodeOptions
    {
        [JsonProperty("type")]
        public int Type { get; set; }
        
        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey { get; set; }
        
        [JsonProperty("receiverPublicKey")]
        public string ReceiverPublicKey { get; set; }
    }
}