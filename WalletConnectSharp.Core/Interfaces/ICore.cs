using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// Represents the Core module and all fields the Core module will have
    /// </summary>
    public interface ICore : IModule, IEvents
    {
        /// <summary>
        /// The Protocol string this Core module will use
        /// </summary>
        public const string Protocol = "wc";
        
        /// <summary>
        /// The Protocol version this Core module will use
        /// </summary>
        public const int Version = 2;
        
        /// <summary>
        /// The Relay URL this Core module will use
        /// </summary>
        public string RelayUrl { get; }
        
        /// <summary>
        /// The project id this Core module will use
        /// </summary>
        public string ProjectId { get; }
        
        //TODO Add logger
        
        /// <summary>
        /// The HeartBeat module this Core module is using. Acts as a consistent interval used for timing
        /// </summary>
        public IHeartBeat HeartBeat { get; }
        
        /// <summary>
        /// The Crypto module this Core module is using. Keeps track of keypairs and executing cryptographic
        /// functions
        /// </summary>
        public ICrypto Crypto { get; }
        
        /// <summary>
        /// The Relayer module this Core module is using. Network layer that acts as relay between wallet / dapp
        /// </summary>
        public IRelayer Relayer { get; }
        
        /// <summary>
        /// The Storage module this Core module is using. Used to store persistant state information between
        /// SDK executions
        /// </summary>
        public IKeyValueStorage Storage { get; }

        /// <summary>
        /// Start the Core module, which will initialize all modules the Core module uses 
        /// </summary>
        public Task Start();
    }
}
