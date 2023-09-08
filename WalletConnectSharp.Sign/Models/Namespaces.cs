using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A dictionary of Namespaces based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class Namespaces : Dictionary<string, Namespace>, IEquatable<Namespaces>
    {
        public Namespaces() : base() { }

        public Namespaces(Namespaces namespaces) : base(namespaces)
        {
            
        }

        public Namespaces(RequiredNamespaces requiredNamespaces)
        {
            WithProposedNamespaces(requiredNamespaces);
        }
        
        public Namespaces(Dictionary<string, ProposedNamespace> proposedNamespaces)
        {
            WithProposedNamespaces(proposedNamespaces);
        }

        public bool Equals(Namespaces other)
        {
            return new DictionaryComparer<string, Namespace>(Namespace.NamespaceComparer).Equals(this, other);
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

            return Equals((Namespaces)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public Namespaces WithNamespace(string chainNamespace, Namespace nm)
        {
            Add(chainNamespace, nm);
            return this;
        }

        public Namespace At(string chainNamespace)
        {
            return this[chainNamespace];
        }
        
        public Namespaces WithProposedNamespaces(Dictionary<string, ProposedNamespace> proposedNamespaces)
        {
            foreach (var pair in proposedNamespaces)
            {
                var chainNamespace = pair.Key;
                var requiredNamespace = pair.Value;
                
                Add(chainNamespace, new Namespace(requiredNamespace));
            }

            return this;
        }
    }
}
