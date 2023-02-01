using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface for the Sign Client. This includes modules the Sign Client will use, the ICore module
    /// this Sign Client is using, as well as public facing Engine functions and properties.
    /// </summary>
    public interface ISignClient : IModule, IEvents, IEngineAPI
    {
        /// <summary>
        /// The Metadata this Sign Client is broadcasting with
        /// </summary>
        Metadata Metadata { get; }
        
        /// <summary>
        /// The <see cref="ICore"/> module this Sign Client is using
        /// </summary>
        ICore Core { get; }
        
        /// <summary>
        /// The <see cref="IEngine"/> module this Sign Client is using
        /// </summary>
        IEngine Engine { get; }
        
        /// <summary>
        /// The <see cref="IPairing"/> module this Sign Client is using for Pairing
        /// </summary>
        IPairing Pairing { get; }
        
        /// <summary>
        /// The <see cref="ISession"/> module this Sign Client is using to store Session data
        /// </summary>
        ISession Session { get; }
        
        /// <summary>
        /// The <see cref="IProposal"/> module this Sign Client is using to store Proposal data
        /// </summary>
        IProposal Proposal { get; }
        
        /// <summary>
        /// The <see cref="IJsonRpcHistoryFactory"/> instance this Sign Client is using to track Json RPC Request / Response
        /// history
        /// </summary>
        IJsonRpcHistoryFactory History { get; }
        
        /// <summary>
        /// The <see cref="IExpirer"/> module this Sign Client is using to track expiration dates
        /// </summary>
        IExpirer Expirer { get; }
        
        /// <summary>
        /// The options this Sign Client was initialized with
        /// </summary>
        SignClientOptions Options { get; }
        
        /// <summary>
        /// The protocol (represented as a string) this Sign Client is using
        /// </summary>
        string Protocol { get; }
        
        /// <summary>
        /// The version of this Sign Client implementation
        /// </summary>
        int Version { get; }
    }
}
