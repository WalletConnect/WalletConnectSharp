using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// A class representing the parameters of an encoded message, all parameters
    /// are raw byte encoded
    /// </summary>
    public class EncodingParams
    {
        /// <summary>
        /// The envelope type as raw bytes 
        /// </summary>
        [JsonProperty("type")]
        public byte[] Type;
        
        /// <summary>
        /// The sealed encoded message as raw bytes
        /// </summary>
        [JsonProperty("sealed")]
        public byte[] Sealed;
        
        /// <summary>
        /// The IV of the encoded message as raw bytes
        /// </summary>
        [JsonProperty("iv")]
        public byte[] Iv;
        
        /// <summary>
        /// The public key of the sender as raw bytes
        /// </summary>
        [JsonProperty("senderPublicKey")]
        public byte[] SenderPublicKey;
    }
}
