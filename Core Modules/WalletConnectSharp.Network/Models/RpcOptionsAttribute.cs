using System;

namespace WalletConnectSharp.Network.Models
{
    public abstract class RpcOptionsAttribute : Attribute
    {
        public long TTL { get; }
        public bool Prompt { get; }
        public int Tag { get; }

        protected RpcOptionsAttribute(long ttl, bool prompt, int tag)
        {
            TTL = ttl;
            Prompt = prompt;
            Tag = tag;
        }
    }
}