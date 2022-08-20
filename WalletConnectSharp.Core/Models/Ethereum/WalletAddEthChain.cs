namespace WalletConnectSharp.Core.Models.Ethereum;

public class WalletAddEthChain : JsonRpcRequest
{
    [JsonProperty("params")]
    private EthChainData[] _parameters;

    [JsonIgnore]
    public EthChainData[] Parameters => _parameters;

    public WalletAddEthChain(EthChainData chainData) : base()
    {
        Method = ValidJsonRpcRequestMethods.WalletAddEthereumChain;
        _parameters = new[] { chainData };
    }
}
