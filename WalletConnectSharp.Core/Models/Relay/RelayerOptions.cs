using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Models.Relay
{
    public class RelayerOptions
    {
        [JsonProperty("core")]
        public ICore Core { get; set; }
        
        [JsonProperty("logger")]
        public string LoggerContext { get; set; }
        
        [JsonProperty("relayUrl")]
        public string RelayUrl { get; set; }
        
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }
    }
}