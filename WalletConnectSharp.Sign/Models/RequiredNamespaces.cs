using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A dictionary of RequiredNamespace based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class RequiredNamespaces : Dictionary<string, ProposedNamespace>, IEquatable<RequiredNamespaces>
    {
        private List<string> _orderedKeys = new();

        public List<string> OrderedKeys => _orderedKeys;
        
        public new void Add(string key, ProposedNamespace value)
        {
            base.Add(key, value);
            _orderedKeys.Add(key);
        }

        public new void Remove(string key)
        {
            base.Remove(key);
            _orderedKeys.Remove(key);
        }
        
        public bool Equals(RequiredNamespaces other)
        {
            return new DictionaryComparer<string, ProposedNamespace>(ProposedNamespace.RequiredNamespaceComparer).Equals(this, other);
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

            return Equals((RequiredNamespaces)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }


        public bool Equals(RequiredNamespaces x, RequiredNamespaces y)
        {
            return new DictionaryComparer<string, ProposedNamespace>(ProposedNamespace.RequiredNamespaceComparer).Equals(x, y);
        }

        public int GetHashCode(RequiredNamespaces obj)
        {
            throw new NotImplementedException();
        }

        public RequiredNamespaces WithProposedNamespace(string chainNamespace, ProposedNamespace proposedNamespace)
        {
            Add(chainNamespace, proposedNamespace);
            return this;
        }
    }
}
