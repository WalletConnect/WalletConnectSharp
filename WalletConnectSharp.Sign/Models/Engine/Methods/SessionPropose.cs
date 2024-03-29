using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionPropose. Used to propose a new session
    /// to be connected to. MUST include a <see cref="RequiredNamespaces"/> and the <see cref="Participant"/> who
    /// is proposing the session
    /// </summary>
    [RpcMethod("wc_sessionPropose")]
    [RpcRequestOptions(Clock.FIVE_MINUTES, 1100)]
    public class SessionPropose : IWcMethod
    {
        /// <summary>
        /// Protocol options that should be used during the session
        /// </summary>
        [JsonProperty("relays")]
        public ProtocolOptions[] Relays;
        
        /// <summary>
        /// The required namespaces this session will require
        /// </summary>
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces;
        
        /// <summary>
        /// The optional namespaces for this session
        /// </summary>
        [JsonProperty("optionalNamespaces")]
        public Dictionary<string, ProposedNamespace> OptionalNamespaces;
        
        /// <summary>
        /// Custom session properties for this session
        /// </summary>
        [JsonProperty("sessionProperties")]
        public Dictionary<string, string> SessionProperties;
        
        /// <summary>
        /// The <see cref="Participant"/> that created this session proposal
        /// </summary>
        [JsonProperty("proposer")]
        public Participant Proposer;
    }
}
