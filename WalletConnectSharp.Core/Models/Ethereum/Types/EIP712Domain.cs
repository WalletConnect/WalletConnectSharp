using WalletConnectSharp.Core.Models.Ethereum.Types;

namespace WalletConnectSharp.Core.Models
{
    public class EIP712Domain
    {
        public string name;
        public string version;
        
        [EvmType("uint256")]
        public int chainId;
        
        public string verifyingContract;

        public EIP712Domain(string name, string version, int chainId, string verifyingContract)
        {
            this.name = name;
            this.version = version;
            this.chainId = chainId;
            this.verifyingContract = verifyingContract;
        }
    }
}