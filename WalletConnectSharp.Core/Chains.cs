using WalletConnectSharp.Core.Models.Ethereum;

namespace WalletConnectSharp.Core
{
    /// <summary>
    /// A static class containing common Chain Ids 
    /// </summary>
    public static class Chains
    {
        /// <summary>
        /// The Ethereum Mainnet Chain with a Chain Id of 0x1
        /// </summary>
        public static EthChain EthereumChainId = new EthChain()
        {
            chainId = "0x1"
        };
        
        /// <summary>
        /// The Expanse Chain with a Chain Id of 0x2
        /// </summary>
        public static EthChain ExpanseChainId = new EthChain()
        {
            chainId = "0x2"
        };
        
        /// <summary>
        /// The Ropsten Chain with a Chain Id of 0x3
        /// </summary>
        public static EthChain RopstenChainId = new EthChain()
        {
            chainId = "0x3"
        };
        
        /// <summary>
        /// The Rinkeby Chain with a Chain Id of 0x4
        /// </summary>
        public static EthChain RinkebyChainId = new EthChain()
        {
            chainId = "0x4"
        };
        
        /// <summary>
        /// The Görli Chain with a Chain Id of 0x5
        /// </summary>
        public static EthChain GörliChainId = new EthChain()
        {
            chainId = "0x5"
        };
        
        /// <summary>
        /// The Kotti Chain with a Chain Id of 0x6
        /// </summary>
        public static EthChain KottiChainId = new EthChain()
        {
            chainId = "0x6"
        };
    }
}