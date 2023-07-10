using Newtonsoft.Json;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Sign.Models.Engine
{
    /// <summary>
    /// A class that represents the options to use to create a session proposal.
    /// </summary>
    public class ConnectOptions
    {
        /// <summary>
        /// The required namespaces that will be required for this session
        /// </summary>
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        /// <summary>
        /// The optional namespaces for this session
        /// </summary>
        [JsonProperty("optionalNamespaces")]
        public Dictionary<string, ProposedNamespace> OptionalNamespaces { get; set; }
        
        /// <summary>
        /// Custom session properties for this session
        /// </summary>
        [JsonProperty("sessionProperties")]
        public Dictionary<string, string> SessionProperties { get; set; }
        
        /// <summary>
        /// The pairing topic to be used to store the session proposal. By default, this should be left blank so
        /// a new pairing topic can be generated. If a pairing topic already exists with the session proposal, then
        /// that pairing topic can be used, however the pairing topic MUST exist in storage.
        /// </summary>
        [JsonProperty("pairingTopic")]
        public string PairingTopic { get; set; }
        
        /// <summary>
        /// The protocol options to use for this session. This is optional and defaults to <see cref="RelayProtocols.Default"/>
        /// value is set.
        /// </summary>
        [JsonProperty("relays")]
        public ProtocolOptions Relays { get; set; }

        /// <summary>
        /// Create blank options with no required namespaces
        /// </summary>
        public ConnectOptions()
        {
            RequiredNamespaces = new RequiredNamespaces();
            SessionProperties = new Dictionary<string, string>();
            OptionalNamespaces = new Dictionary<string, ProposedNamespace>();
        }
        
        /// <summary>
        /// Create new options
        /// </summary>
        /// <param name="requiredNamespaces">The required namespaces that will be needed for this session. Defaults to none</param>
        /// <param name="pairingTopic">The pairing topic to use. Defaults to a random pairing topic that will be generated when submitted</param>
        /// <param name="relays">The relay protocol options to use. Defaults to <see cref="RelayProtocols.Default"/></param>
        public ConnectOptions(RequiredNamespaces requiredNamespaces = null, string pairingTopic = null,
            ProtocolOptions relays = null, Dictionary<string, ProposedNamespace> optionalNamespaces = null, Dictionary<string, string> sessionProperties = null)
        {
            RequiredNamespaces = requiredNamespaces ?? new RequiredNamespaces();
            OptionalNamespaces = optionalNamespaces ?? new Dictionary<string, ProposedNamespace>();
            SessionProperties = sessionProperties ?? new Dictionary<string, string>();
            PairingTopic = pairingTopic ?? "";
            Relays = relays;
        }

        /// <summary>
        /// Require a specific chain and namespace
        /// </summary>
        /// <param name="chain">The chain the namespace exists in</param>
        /// <param name="proposedNamespace">The required namespace that must be present for this session</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions RequireNamespace(string chain, ProposedNamespace proposedNamespace)
        {
            RequiredNamespaces.Add(chain, proposedNamespace);

            return this;
        }
        
        /// <summary>
        /// Specify an optional chain and namespace
        /// </summary>
        /// <param name="chain">The chain the namespace exists in</param>
        /// <param name="proposedNamespace">The required namespace that must be present for this session</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions WithOptionalNamespace(string chain, ProposedNamespace proposedNamespace)
        {
            OptionalNamespaces.Add(chain, proposedNamespace);

            return this;
        }
        
        /// <summary>
        /// Set a session property under a given key
        /// </summary>
        /// <param name="key">The key of the session property to add</param>
        /// <param name="value">The value of the session property to add, will be stored under the given key</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions AddSessionProperty(string key, string value)
        {
            SessionProperties.Add(key, value);

            return this;
        }
        
        /// <summary>
        /// Set a session property under a given key
        /// </summary>
        /// <param name="key">The key of the session property to add</param>
        /// <param name="value">The value of the session property to add, will be stored under the given key</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions WithSessionProperties(Dictionary<string, string> properties)
        {
            SessionProperties = properties;

            return this;
        }

        /// <summary>
        /// Include a pairing topic with these connect options. The pairing topic MUST exist in storage.
        /// </summary>
        /// <param name="pairingTopic">The pairing topic to include</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions WithPairingTopic(string pairingTopic)
        {
            PairingTopic = pairingTopic;
            return this;
        }

        /// <summary>
        /// Include protocol options
        /// </summary>
        /// <param name="options">The options to include</param>
        /// <returns>This object, acts a builder function</returns>
        public ConnectOptions WithOptions(ProtocolOptions options)
        {
            Relays = options;
            return this;
        }
    }
}
