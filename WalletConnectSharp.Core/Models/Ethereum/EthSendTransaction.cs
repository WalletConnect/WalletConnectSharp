namespace WalletConnectSharp.Core.Models.Ethereum;

public sealed class EthSendTransaction : JsonRpcRequest
{
    [JsonProperty("params")]
    private TransactionData[] _parameters;

    [JsonIgnore]
    public TransactionData[] Parameters => _parameters;

    public EthSendTransaction(params TransactionData[] transactionDatas) : base()
    {
        Method = ValidJsonRpcRequestMethods.EthSendTransaction;
        _parameters = transactionDatas;
    }
}
