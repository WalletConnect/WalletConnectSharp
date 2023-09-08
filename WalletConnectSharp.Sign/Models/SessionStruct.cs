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
    }
}
