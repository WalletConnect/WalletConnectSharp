using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class IridiumJWTSigned : IridiumJWTData
    {
        [JsonProperty("signature")]
        public byte[] Signature { get; set; }
    }
}