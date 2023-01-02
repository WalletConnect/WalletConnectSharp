using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class Participant
    {
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
        
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }
}