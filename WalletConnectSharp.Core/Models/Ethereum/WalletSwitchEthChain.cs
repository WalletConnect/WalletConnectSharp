namespace WalletConnectSharp.Core.Models.Ethereum;

public class WalletSwitchEthChain : JsonRpcRequest
{
    [JsonProperty("params")]
    private EthChain[] _parameters;

    [JsonIgnore]
    public EthChain[] Parameters => _parameters;

    public WalletSwitchEthChain(params EthChain[] chainId) : base()
    {
        Method = ValidJsonRpcRequestMethods.WalletSwitchEthereumChain;
        _parameters = chainId;
    }
}