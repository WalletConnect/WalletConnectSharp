using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A required namespace that holds chains, methods and events enabled.
    /// </summary>
    public class RequiredNamespace
    {
        /// <summary>
        /// A list of all chains that are required to be enabled in this namespace
        /// </summary>
        [JsonProperty("chains")]
        public string[] Chains { get; set; }
        
        /// <summary>
        /// A list of all methods that are required to be enabled in this namespace
        /// </summary>
        [JsonProperty("methods")]
        public string[] Methods { get; set; }
        
        /// <summary>
        /// A list of all events that are required to be enabled in this namespace
        /// </summary>
        [JsonProperty("events")]
        public string[] Events { get; set; }

        /// <summary>
        /// Create a blank required namespace
        /// </summary>
        public RequiredNamespace()
        {
            Chains = Array.Empty<string>();
            Methods = Array.Empty<string>();
            Events = Array.Empty<string>();
        }

        /// <summary>
        /// Add a chainId as required in this namespace
        /// </summary>
        /// <param name="chain">The chain to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public RequiredNamespace WithChain(string chain)
        {
            Chains = Chains.Append(chain).ToArray();
            return this;
        }

        /// <summary>
        /// Add a method as required in this namespace
        /// </summary>
        /// <param name="method">The method name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public RequiredNamespace WithMethod(string method)
        {
            Methods = Methods.Append(method).ToArray();
            return this;
        }
        
        /// <summary>
        /// Add an event as required in this namespace
        /// </summary>
        /// <param name="event">The event name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public RequiredNamespace WithEvent(string @event)
        {
            Events = Events.Append(@event).ToArray();
            return this;
        }
    }
}
