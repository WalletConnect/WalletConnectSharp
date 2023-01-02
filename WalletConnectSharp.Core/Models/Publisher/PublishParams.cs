using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Publisher
{
    public class PublishParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("opts")]
        public PublishOptions Options { get; set; }
    }
}