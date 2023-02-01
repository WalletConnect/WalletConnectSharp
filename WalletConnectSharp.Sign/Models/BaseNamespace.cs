using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A base namespace that holds accounts, methods and events enabled
    /// </summary>
    public class BaseNamespace
    {
        /// <summary>
        /// An array of all accounts enabled in this namespace
        /// </summary>
        [JsonProperty("accounts")]
        public string[] Accounts { get; set; }
        
        /// <summary>
        /// An array of all methods enabled in this namespace
        /// </summary>
        [JsonProperty("methods")]
        public string[] Methods { get; set; }
        
        /// <summary>
        /// An array of all events enabled in this namespace
        /// </summary>
        [JsonProperty("events")]
        public string[] Events { get; set; }
    }
}
