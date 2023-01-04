using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class ConnectOptions
    {
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        [JsonProperty("pairingTopic")]
        public string PairingTopic { get; set; }
        
        [JsonProperty("relays")]
        public ProtocolOptions Relays { get; set; }

        public ConnectOptions()
        {
            RequiredNamespaces = new RequiredNamespaces();
        }
        
        public ConnectOptions RequireNamespace(string chain, RequiredNamespace requiredNamespace)
        {
            RequiredNamespaces.Add(chain, requiredNamespace);

            return this;
        }

        public ConnectOptions WithPairingTopic(string pairingTopic)
        {
            PairingTopic = pairingTopic;
            return this;
        }

        public ConnectOptions WithOptions(ProtocolOptions options)
        {
            Relays = options;
            return this;
        }
    }
}