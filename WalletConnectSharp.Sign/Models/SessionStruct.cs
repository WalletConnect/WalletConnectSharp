using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models
{
    public struct SessionStruct : IKeyHolder<string>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        [JsonProperty("expiry")]
        public long? Expiry { get; set; }
        
        [JsonProperty("acknowledged")]
        public bool? Acknowledged { get; set; }
        
        [JsonProperty("controller")]
        public string Controller { get; set; }
        
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
        
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        [JsonProperty("self")]
        public Participant Self { get; set; }
        
        [JsonProperty("peer")]
        public Participant Peer { get; set; }

        [JsonIgnore]
        public string Key
        {
            get
            {
                return Topic;
            }
        }
    }
}