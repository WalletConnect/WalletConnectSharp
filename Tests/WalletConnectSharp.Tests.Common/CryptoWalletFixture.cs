using NBitcoin;
using Nethereum.HdWallet;

namespace WalletConnectSharp.Auth.Tests;

public class CryptoWalletFixture
{
    private readonly Wallet _wallet;
    private readonly string _iss;

    public string WalletAddress
    {
        get
        {
            return _wallet.GetAddresses(0)[0];
        }
    }

    public Wallet CryptoWallet
    {
        get
        {
            return _wallet;
        }
    }

    public string Iss
    {
        get
        {
            return _iss;
        }
    }
    
    public CryptoWalletFixture()
    {
        this._wallet = new Wallet(Wordlist.English, WordCount.Twelve);
        this._iss = $"did:pkh:eip155:1:{this.WalletAddress}";
    }
}
