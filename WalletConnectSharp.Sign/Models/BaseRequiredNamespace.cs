using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A base required namespace that includes multiple builder functions
    /// to help construct a required namespace list. This class is generic and expects
    /// a subclass to inherit this class, marking its own type as T. This is to help
    /// the builder functions in this class return the correct subclass type.
    /// </summary>
    /// <typeparam name="T">The subclass inheriting this class</typeparam>
    public class BaseRequiredNamespace<T> where T : BaseRequiredNamespace<T>
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
        public BaseRequiredNamespace()
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
        public T WithChain(string chain)
        {
            Chains = Chains.Append(chain).ToArray();
            return this as T;
        }

        /// <summary>
        /// Add a method as required in this namespace
        /// </summary>
        /// <param name="method">The method name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public T WithMethod(string method)
        {
            Methods = Methods.Append(method).ToArray();
            return this as T;
        }
        
        /// <summary>
        /// Add an event as required in this namespace
        /// </summary>
        /// <param name="event">The event name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public T WithEvent(string @event)
        {
            Events = Events.Append(@event).ToArray();
            return this as T;
        }
    }
}
