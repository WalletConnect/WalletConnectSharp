using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A required namespace that holds chains, methods and events enabled. Also includes
    /// extension required namespaces that are enabled as well
    /// </summary>
    public class RequiredNamespace : BaseRequiredNamespace<RequiredNamespace>
    {
        /// <summary>
        /// An array of required extension namespaces that are enabled
        /// </summary>
        [JsonProperty("extension", NullValueHandling=NullValueHandling.Ignore)]
        public BaseRequiredNamespace<RequiredNamespace>[] Extension { get; set; }

        /// <summary>
        /// Create a new blank required namespace
        /// </summary>
        public RequiredNamespace() : base()
        {
            Extension = null;
        }

        /// <summary>
        /// Add an extension required namespace to this required namespace
        /// </summary>
        /// <param name="requiredNamespace">The extension required namespace to add</param>
        /// <returns>This object, acts a builder function</returns>
        public RequiredNamespace WithExtension(BaseRequiredNamespace<RequiredNamespace> requiredNamespace)
        {
            if (Extension == null)
                Extension = Array.Empty<BaseRequiredNamespace<RequiredNamespace>>().Append(requiredNamespace).ToArray();
            else
                Extension = Extension.Append(requiredNamespace).ToArray();
            return this;
        }
    }
}
