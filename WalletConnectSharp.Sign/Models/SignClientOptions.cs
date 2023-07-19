using Newtonsoft.Json;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// Options for setting up the <see cref="BaseWalletConnectSignClient"/> class. Includes
    /// options from <see cref="CoreOptions"/>
    /// </summary>
    public class SignClientOptions : CoreOptions
    {
        /// <summary>
        /// The <see cref="ICore"/> instance the <see cref="BaseWalletConnectSignClient"/> should use. If
        /// left null, then a new Core module will be created and initialized
        /// </summary>
        [JsonProperty("core")]
        public ICore Core { get; set; }
        
        /// <summary>
        /// The Metadata the <see cref="BaseWalletConnectSignClient"/> should broadcast with 
        /// </summary>
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }
}
