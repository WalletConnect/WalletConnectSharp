using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// A class representing the encoding parameters to validate
    /// </summary>
    public class EncodingValidation
    {
        /// <summary>
        /// The envelope type to validate
        /// </summary>
        [JsonProperty("type")]
        public int Type;
        
        /// <summary>
        /// The sender public key to validate
        /// </summary>
        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey;
        
        /// <summary>
        /// The receiver public key to validate
        /// </summary>
        [JsonProperty("receiverPublicKey")]
        public string ReceiverPublicKey;
    }
}
