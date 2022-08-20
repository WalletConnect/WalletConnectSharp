namespace WalletConnectSharp.Core.Models;

public static class JsonRpcRequestMethods
{
    public static JsonRpcRequestMethod[] Methods { get; } = new[] {
        // Signing methods
        new JsonRpcRequestMethod() { Name = ValidJsonRpcRequestMethods.EthSendTransaction, IsSigning = true, },
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

    private static readonly string WalletConnectMethodPrefix = "wc_";

    public static readonly JsonRpcRequestMethod[] SigningMethods = Array.FindAll(Methods, m => m.IsSigning);

    public static readonly JsonRpcRequestMethod[] RedirectMethods = Array.FindAll(Methods, m => m.IsRedirect);

    public static bool IsSigningMethod(string method) => Array.Exists(SigningMethods, wcMethod => wcMethod.Name == method);
    public static bool IsWalletConnectMethod(string method) => method.StartsWith(WalletConnectMethodPrefix);
    public static string[] ToStringArray(this JsonRpcRequestMethod[] methods) => methods.Select(m => m.ToString()).ToArray();
}

public static class ValidJsonRpcRequestMethods
{
    public static readonly string EthSendTransaction = "eth_sendTransaction";
    public static readonly string EthSignTransaction = "eth_signTransaction";
    public static readonly string EthSign = "eth_sign";
    public static readonly string EthSignTypedData = "eth_signTypedData";
    public static readonly string EthSignTypedDataV1 = "eth_signTypedData_v1";
    public static readonly string EthSignTypedDataV2 = "eth_signTypedData_v2";
    public static readonly string EthSignTypedDataV3 = "eth_signTypedData_v3";
    public static readonly string EthSignTypedDataV4 = "eth_signTypedData_v4";
    public static readonly string PersonalSign = "personal_sign";
    public static readonly string WalletAddEthereumChain = "wallet_addEthereumChain";
    public static readonly string WalletSwitchEthereumChain = "wallet_switchEthereumChain";
}