using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Test.Shared {

    public static class SignTestValues
    {
        public static readonly string TestPolkadotAddress = "8cGfbK9Q4zbsNzhZsZUtpsQgX5LG2UCPEDuXYV33whktGt7";
        public static readonly string TestEthereumAddress = "0x3c582121909DE92Dc89A36898633C1aE4790382b";
        public static readonly string TestEthereumChain = "eip155:1";
        public static readonly string TestArbitrumChain = "eip155:42161";
        public static readonly string TestAvalancheChain = "eip155:43114";
        public static readonly string TestPolkadotChain = "polkadot:91b171bb158e2d3848fa23a9f1c25182";
        public static readonly string TestPolkadotAccount = $"{TestPolkadotChain}:{TestPolkadotAddress}";

        public static readonly string[] TestAccounts = new[]
        {
            $"{TestEthereumChain}:{TestEthereumAddress}", $"{TestArbitrumChain}:{TestEthereumAddress}",
            $"{TestAvalancheChain}:{TestEthereumAddress}"
        };

        public static readonly string[] TestMethods = new[]
        {
            "eth_sendTransaction", "eth_signTransaction", "personal_sign", "eth_signTypedData",
        };

        public static readonly string[] TestPolkadotMethods = new[]
        {
            "polkadot_signTransaction",
            "polkadot_signMessage"
        };

        public static readonly string[] TestChains = new[] { TestEthereumChain, TestArbitrumChain, };
        
        public static readonly string[] TestEvents = new[] { "chainChanged", "accountsChanged" };

        public static readonly RequiredNamespaces TestRequiredNamespacees = new RequiredNamespaces()
        {
            {
                "eip155", new ProposedNamespace()
                {
                    Methods = TestMethods,
                    Events = TestEvents,
                    Chains = TestChains
                }
            }
        };
        
        public static readonly RequiredNamespaces TestRequiredNamespaceesV2 = new RequiredNamespaces()
        {
            {
                "eip155", new ProposedNamespace()
                {
                    Methods = TestMethods,
                    Events = TestEvents,
                    Chains = TestChains
                }
            },
            {
                TestAvalancheChain, new ProposedNamespace()
                {
                    Methods = TestMethods,
                    Events = TestEvents
                }
            }
        };

        public static readonly Namespaces TestOptionalNamespaces = new Namespaces()
        {
            {
                "polkadot", new Namespace()
                {
                    
                }
            }
        };
    }
}
