using WalletConnectSharp.Auth.Internals;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Tests.Common;
using Xunit;

namespace WalletConnectSharp.Auth.Tests;

public class SignatureTest
{
    public string ChainId = "eip155:1";
    public string ProjectId = TestValues.TestProjectId;
    public string Address = "0x2faf83c542b68f1b4cdc0e770e8cb9f567b08f71";

    public string ReconstructedMessage = @"localhost wants you to sign in with your Ethereum account:
0x2faf83c542b68f1b4cdc0e770e8cb9f567b08f71

URI: http://localhost:3000/
Version: 1
Chain ID: 1
Nonce: 1665443015700
Issued At: 2022-10-10T23:03:35.700Z
Expiration Time: 2022-10-11T23:03:35.700Z".Replace("\r", "");
    
    [Fact, Trait("Category", "unit")]
    public async Task TestValidEip1271Signature()
    {
        var signature = new Cacao.CacaoSignature.EIP1271CacaoSignature(
            "0xc1505719b2504095116db01baaf276361efd3a73c28cf8cc28dabefa945b8d536011289ac0a3b048600c1e692ff173ca944246cf7ceb319ac2262d27b395c82b1c");

        var isValid =
            await SignatureUtils.VerifySignature(Address, ReconstructedMessage, signature, ChainId, ProjectId);
        
        Assert.True(isValid);
    }

    [Fact, Trait("Category", "unit")]
    public async Task TestBadEip1271Signature()
    {
        var signature = new Cacao.CacaoSignature.EIP1271CacaoSignature(
            "0xdead5719b2504095116db01baaf276361efd3a73c28cf8cc28dabefa945b8d536011289ac0a3b048600c1e692ff173ca944246cf7ceb319ac2262d27b395c82b1c");

        var isValid =
            await SignatureUtils.VerifySignature(Address, ReconstructedMessage, signature, ChainId, ProjectId);
        
        Assert.False(isValid);
    }
}
