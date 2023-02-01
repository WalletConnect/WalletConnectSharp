using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A namespace that holds accounts, methods and events enabled. Also includes
    /// extension namespaces that are enabled
    /// </summary>
    public class Namespace : BaseNamespace
    {
        /// <summary>
        /// Any extension namespaces that are enabled in this namespace
        /// </summary>
        [JsonProperty("extension", NullValueHandling=NullValueHandling.Ignore)]
        public BaseNamespace[] Extension { get; set; }
    }
}
