using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    public class IridiumJWTData
    {
        [JsonProperty("header")]
        public IridiumJWTHeader Header { get; set; }
        
        [JsonProperty("payload")]
        public IridiumJWTPayload Payload { get; set; }
    }
}