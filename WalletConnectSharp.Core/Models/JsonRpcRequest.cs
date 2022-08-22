using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Models;

public class JsonRpcRequest : IEventSource
{
    [JsonIgnore]
    public static JsonRpcRequestMethod[] Methods { get; } = new[] {
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthFeeHistory},
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSendTransaction, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSendRawTransaction },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSign, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTransaction, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTypedData, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTypedDataV1, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTypedDataV2, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTypedDataV3, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSignTypedDataV4, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.PersonalSign, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.WalletAddEthereumChain, IsSigning = true, },
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.WalletSwitchEthereumChain, IsSigning = true },
    };

    [JsonIgnore]
    private static readonly string WalletConnectMethodPrefix = "wc_";

    [JsonIgnore]
    public static readonly JsonRpcRequestMethod[] SigningMethods = Array.FindAll(Methods, m => m.IsSigning);

    [JsonIgnore]
    public static readonly JsonRpcRequestMethod[] RedirectMethods = Array.FindAll(Methods, m => m.IsRedirect);

    [JsonProperty(Order = 1)]
    private readonly long id;
    [JsonProperty(Order = 2)]
    private const string JSONRPC = "2.0";

    [JsonProperty("method", Order = 3)]
    public virtual string Method { get; protected set; }

    public JsonRpcRequest()
    {
        if (id == 0)
        {
            id = RpcPayloadId.Generate();
        }
    }

    public static bool IsSigningMethod(string method) => Array.Exists(SigningMethods, wcMethod => wcMethod.Name == method);
    public static bool IsWalletConnectMethod(string method) => method.StartsWith(WalletConnectMethodPrefix);

    [JsonIgnore]
    public long ID => id;

    [JsonIgnore]
    public string JsonRPC => JSONRPC;

    [JsonIgnore]
    public string Event => Method;
}
