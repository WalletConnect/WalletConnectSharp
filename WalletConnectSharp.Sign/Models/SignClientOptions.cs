using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Sign.Models
{
    public class SignClientOptions : CoreOptions
    {
        [JsonProperty("core")]
        public ICore Core { get; set; }
        
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }
}