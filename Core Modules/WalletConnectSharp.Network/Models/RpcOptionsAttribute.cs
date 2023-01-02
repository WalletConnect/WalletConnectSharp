using System;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// An attribute that defines the RPC options for this class. These options
    /// are used when this class type is used as a parameter or result of a Json RPC
    /// request/response. This cannot be used directly, use either
    /// * RpcRequestOptions
    /// * RpcResponseOptions
    /// </summary>
    public abstract class RpcOptionsAttribute : Attribute
    {
        /// <summary>
        /// The TTL (time to live)
        /// </summary>
        public long TTL { get; }
        
        /// <summary>
        /// Whether this request should prompt the user
        /// </summary>
        public bool Prompt { get; }
        
        /// <summary>
        /// The Tag
        /// </summary>
        public int Tag { get; }

        /// <summary>
        /// Create a new RpcOptions attribute with the given ttl, prompt and tag paramaters
        /// </summary>
        /// <param name="ttl">Time to live</param>
        /// <param name="prompt">Whether the user should be prompted</param>
        /// <param name="tag">The tag</param>
        protected RpcOptionsAttribute(long ttl, bool prompt, int tag)
        {
            TTL = ttl;
            Prompt = prompt;
            Tag = tag;
        }
    }
}
