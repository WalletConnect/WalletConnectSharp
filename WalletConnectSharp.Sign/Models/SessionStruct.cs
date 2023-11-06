using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A struct that holds session data, including the session topic, when the session expires, whether the session has
    /// been acknowledged and who the session controller is.
    /// </summary>
    public struct SessionStruct : IKeyHolder<string>
    {
        /// <summary>
        /// The topic of this session
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;
        
        /// <summary>
        /// The pairing topic of this session
        /// </summary>
        [JsonProperty("pairingTopic")]
        public string PairingTopic;
        
        /// <summary>
        /// The relay protocol options this session is using
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay;
        
        /// <summary>
        /// When this session expires
        /// </summary>
        [JsonProperty("expiry")]
        public long? Expiry;
        
        /// <summary>
        /// Whether this session has been acknowledged or not
        /// </summary>
        [JsonProperty("acknowledged")]
        public bool? Acknowledged;
        
        /// <summary>
        /// The public key of the current controller for this session
        /// </summary>
        [JsonProperty("controller")]
        public string Controller;
        
        /// <summary>
        /// The enabled namespaces this session uses
        /// </summary>
        [JsonProperty("namespaces")]
        public Namespaces Namespaces;
        
        /// <summary>
        /// The required enabled namespaces this session uses
        /// </summary>
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces;
   
        /// <summary>
        /// The <see cref="Participant"/> data that represents ourselves in this session
        /// </summary>
        [JsonProperty("self")]
        public Participant Self;
        
        /// <summary>
        /// The <see cref="Participant"/> data that represents the peer in this session
        /// </summary>
        [JsonProperty("peer")]
        public Participant Peer;

        /// <summary>
        /// This is the key field, mapped to the Topic. Implemented for <see cref="IKeyHolder{TKey}"/>
        /// so this struct can be stored using <see cref="IStore{TKey,TValue}"/>
        /// </summary>
        [JsonIgnore]
        public string Key
        {
            get
            {
                return Topic;
            }
        }
        
        public Caip25Address CurrentAddress(string @namespace)
        {
            // double check
            if (@namespace == null)
                throw new ArgumentException("SessionStruct.CurrentAddress: @namespace is null");
            if (string.IsNullOrWhiteSpace(Topic))
                throw new ArgumentException("SessionStruct.CurrentAddress: Session is undefined");
        
            var defaultNamespace = Namespaces[@namespace];

            if (defaultNamespace.Accounts.Length == 0)
                throw new Exception(
                    $"SessionStruct.CurrentAddress: Given namespace {@namespace} has no connected addresses");

            var fullAddress = defaultNamespace.Accounts[0];
            var addressParts = fullAddress.Split(":");

            var address = addressParts[2];
            var chainId = string.Join(':', addressParts.Take(2));

            return new Caip25Address()
            {
                Address = address,
                ChainId = chainId,
            };
        }
        
        public Caip25Address[] AllAddresses(string @namespace)
        {
            // double check
            if (@namespace == null)
                throw new ArgumentException("SessionStruct.AllAddresses: @namespace is null");
            if (string.IsNullOrWhiteSpace(Topic))
                throw new ArgumentException("SessionStruct.AllAddresses: Session is undefined");
        
            var defaultNamespace = Namespaces[@namespace];

            if (defaultNamespace.Accounts.Length == 0)
                return null; //The namespace {@namespace} has no addresses connected")

            return defaultNamespace.Accounts.Select(addr => new Caip25Address()
            {
                Address = addr.Split(":")[2], ChainId = string.Join(":", addr.Split(":").Take(2))
            }).ToArray();
        }
    }
}
