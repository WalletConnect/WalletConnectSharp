using System;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents the Engine for running the Sign client. This interface
    /// is an sub-type of <see cref="IEngineAPI"/> and represents the actual Engine. This is
    /// different than the Sign client.
    /// </summary>
    public interface IEngine : IEngineAPI, IDisposable
    {
        /// <summary>
        /// The <see cref="ISignClient"/> this Engine is using
        /// </summary>
        ISignClient Client { get; } 

        /// <summary>
        /// Initialize the Engine. This loads any persistant state and connects to the WalletConnect
        /// relay server 
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Parse a session proposal URI and return all information in the URI. 
        /// </summary>
        /// <param name="uri">The URI to parse</param>
        /// <returns>The parameters parsed from the URI</returns>
        UriParameters ParseUri(string uri);
    }
}
