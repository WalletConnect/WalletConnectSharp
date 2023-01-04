using Newtonsoft.Json;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core.Models
{
    public class CoreOptions
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("relayUrl")]
        public string RelayUrl { get; set; }
        
        [JsonProperty("logger")]
        public string LoggerContext { get; set; }

        [JsonProperty("keychain")]
        public IKeyChain KeyChain { get; set; }
        
        [JsonProperty("storage")]
        public IKeyValueStorage Storage { get; set; }
    }
}