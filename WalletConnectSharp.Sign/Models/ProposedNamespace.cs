using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A required namespace that holds chains, methods and events enabled.
    /// </summary>
    public class ProposedNamespace
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
        public ProposedNamespace()
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
        public ProposedNamespace WithChain(string chain)
        {
            Chains = Chains.Append(chain).ToArray();
            return this;
        }

        /// <summary>
        /// Add a method as required in this namespace
        /// </summary>
        /// <param name="method">The method name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public ProposedNamespace WithMethod(string method)
        {
            Methods = Methods.Append(method).ToArray();
            return this;
        }
        
        /// <summary>
        /// Add an event as required in this namespace
        /// </summary>
        /// <param name="event">The event name to add</param>
        /// <returns>This object, acts as a builder function</returns>
        public ProposedNamespace WithEvent(string @event)
        {
            Events = Events.Append(@event).ToArray();
            return this;
        }

        protected bool Equals(ProposedNamespace other)
        {
            return Equals(Chains, other.Chains) && Equals(Methods, other.Methods) && Equals(Events, other.Events);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ProposedNamespace)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Chains, Methods, Events);
        }

        private sealed class RequiredNamespaceEqualityComparer : IEqualityComparer<ProposedNamespace>
        {
            public bool Equals(ProposedNamespace x, ProposedNamespace y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Chains.SequenceEqual(y.Chains) && x.Methods.SequenceEqual(y.Methods) && x.Events.SequenceEqual(y.Events);
            }

            public int GetHashCode(ProposedNamespace obj)
            {
                return HashCode.Combine(obj.Chains, obj.Methods, obj.Events);
            }
        }

        public static IEqualityComparer<ProposedNamespace> RequiredNamespaceComparer { get; } = new RequiredNamespaceEqualityComparer();
    }
}
