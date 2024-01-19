namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A dictionary of Namespaces based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class Namespaces : SortedDictionary<string, Namespace>
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

        public Namespaces WithNamespace(string chainNamespace, Namespace nm)
        {
            Add(chainNamespace, nm);
            return this;
        }

        public Namespace At(string chainNamespace)
        {
            return this[chainNamespace];
        }

        public Namespaces WithProposedNamespaces(IDictionary<string, ProposedNamespace> proposedNamespaces)
        {
            foreach (var (chainNamespace, requiredNamespace) in proposedNamespaces)
            {
                Add(chainNamespace, new Namespace(requiredNamespace));
            }

            return this;
        }
    }
}
