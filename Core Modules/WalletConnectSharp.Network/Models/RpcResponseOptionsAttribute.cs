using System;

namespace WalletConnectSharp.Network.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcResponseOptionsAttribute : RpcOptionsAttribute
    {
        public RpcResponseOptionsAttribute(long ttl, bool prompt, int tag) : base(ttl, prompt, tag)
        {
        }
    }
}