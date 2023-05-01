using System;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// An attribute that defines the RPC options for this class (or struct). These options
    /// are used when this class type is used as a parameter of a Json RPC
    /// request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcRequestOptionsAttribute : RpcOptionsAttribute
    {
        public RpcRequestOptionsAttribute(long ttl, int tag) : base(ttl, tag)
        {
        }
    }
}
