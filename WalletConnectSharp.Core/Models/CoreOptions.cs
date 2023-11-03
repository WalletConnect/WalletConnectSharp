using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Network.Interfaces;
using WalletConnectSharp.Platform;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core.Models
{
    /// <summary>
    /// Options used to configure the Core module. 
    /// </summary>
    public class CoreOptions
    {
        /// <summary>
        /// The Project ID to use to authenticate with the relay server
        /// </summary>
        [JsonProperty("projectId")]
        public string ProjectId;

        /// <summary>
        /// The name that this Core module will show itself as
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// The URL of the relay server to connect to. This should not include any auth info
        /// </summary>
        [JsonProperty("relayUrl")]
        public string RelayUrl;
        
        /// <summary>
        /// The base context string to use for module isolation. If null or empty, then the string
        /// "{Name}-client" will be used
        /// </summary>
        [JsonProperty("context")]
        public string BaseContext;
        
        /// <summary>
        /// The <see cref="IKeyValueStorage"/> module to use for storage. This module will be used by most Core modules
        /// for storing data and by the default <see cref="IKeyChain"/> module (if no <see cref="IKeyChain"/> module is provided).
        /// If this is set to null, then the default <see cref="FileSystemStorage"/> 
        /// </summary>
        [JsonProperty("storage")]
        public IKeyValueStorage Storage;

        /// <summary>
        /// The <see cref="IKeyChain"/> module to use for the <see cref="ICrypto"/> module.
        /// If set to null, then the default <see cref="KeyChain"/> module will be used with the provided Storage module.
        /// </summary>
        [JsonProperty("keychain")]
        public IKeyChain KeyChain;
        
        /// <summary>
        /// The <see cref="IPlatform"/> interface to use inside in the Platform API
        /// </summary>
        [JsonProperty("platform")]
        public IPlatform Platform;
        
        /// <summary>
        /// The <see cref="ICrypto"/> module to use for crypto operations. This option
        /// overrides the KeyChain option. If set to null, then a default Crypto module will be used
        /// with either the KeyChain option or a default keychain
        /// </summary>
        public ICrypto CryptoModule;
        
    }
}
