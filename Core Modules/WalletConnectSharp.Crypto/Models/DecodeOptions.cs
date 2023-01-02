using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class DecodeOptions
    {
        [JsonProperty("receiverPublicKey")]
        public string ReceiverPublicKey { get; set; }
    }
}