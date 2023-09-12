using System.Text;
using Nethereum.Signer;
using Newtonsoft.Json;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Eth;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Internals;

public class SignatureUtils
{
    public const string DefaultRpcUrl = "https://rpc.walletconnect.com/v1";
    
    public static async Task<bool> VerifySignature(string address, string reconstructedMessage, Cacao.CacaoSignature cacaoSignature,
        string chainId, string projectId)
    {
        switch (cacaoSignature.T)
        {
            case "eip191":
                return IsValidEip191Signature(address, reconstructedMessage, cacaoSignature.S);
            case "eip1271":
                return await IsValidEip1271Signature(address, reconstructedMessage, cacaoSignature.S, chainId, projectId);
            default:
                throw new ArgumentException(
                    $"VerifySignature Failed: Attempted to verify CacaoSignature with unknown type {cacaoSignature.T}");
        }
    }

    private static async Task<bool> IsValidEip1271Signature(string address, string reconstructedMessage, string cacaoSignatureS, string chainId, string projectId)
    {
        var eip1271MagicValue = "0x1626ba7e";
        var dynamicTypeOffset = "0000000000000000000000000000000000000000000000000000000000000040";
        var dynamicTypeLength = "0000000000000000000000000000000000000000000000000000000000000041";
        var nonPrefixedSignature = cacaoSignatureS.Substring(2);
        var signer = new EthereumMessageSigner();
        var nonPrefixedHashedMessage = signer.HashPrefixedMessage(Encoding.UTF8.GetBytes(reconstructedMessage)).ToHex();

        var data =
            eip1271MagicValue +
            nonPrefixedHashedMessage +
            dynamicTypeOffset +
            dynamicTypeLength +
            nonPrefixedSignature;

        string result = null;
        using (var client = new HttpClient())
        {
            var url = $"{DefaultRpcUrl}/?chainId={chainId}&projectId={projectId}";

            var rpcRequest = new JsonRpcRequest<object[]>("eth_call",
                new object[] { new EthCall() { To = address, Data = data }, "latest" });
            
            var httpResponse = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(rpcRequest)));

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<JsonRpcResponse<string>>(jsonResponse);

            if (response != null)
                result = response.Result;
            else
                throw new Exception($"Could not deserialize JsonRpcResponse from JSON {jsonResponse}");
        }
            
        if (string.IsNullOrWhiteSpace(result) || result == "0x")
        {
            return false;
        }

        var recoveredValue = result.Substring(0, eip1271MagicValue.Length);
        return recoveredValue.ToLower() == eip1271MagicValue.ToLower();
    }

    private static bool IsValidEip191Signature(string address, string reconstructedMessage, string cacaoSignatureS)
    {
        var signer = new EthereumMessageSigner();
        var recoveredAddress = signer.EncodeUTF8AndEcRecover(reconstructedMessage, cacaoSignatureS);
        return String.Equals(recoveredAddress, address, StringComparison.CurrentCultureIgnoreCase);
    }
}
