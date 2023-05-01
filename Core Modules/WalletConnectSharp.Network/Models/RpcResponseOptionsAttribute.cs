using System;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// An attribute that defines the RPC options for this class (or struct). These options
    /// are used when this class type is used as a result of a Json RPC
    /// response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcResponseOptionsAttribute : RpcOptionsAttribute
    {
        public RpcResponseOptionsAttribute(long ttl, int tag) : base(ttl, tag)
        {
        }
    }
}
