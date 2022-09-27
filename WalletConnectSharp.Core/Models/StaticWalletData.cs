using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Models;

public class StaticWalletData : IWalletData
{
    public ObservableValue<string[]> AccountsObservable { get; }
    public ObservableValue<int> ChainIdObservable { get; }
    public ObservableValue<int> NetworkIdObservable { get; }
    
    public ClientMeta ClientMeta { get; set; }

    public string[] Accounts
    {
        get
        {
            return AccountsObservable.Value;
        }
        set
        {
            AccountsObservable.Value = value;
        }
    }

    public int ChainId
    {
        get
        {
            return ChainIdObservable.Value;
        }
        set => ChainIdObservable.Value = value;
    }

    public int NetworkId
    {
        get
        {
            return NetworkIdObservable.Value;
        }
        set
        {
            NetworkIdObservable.Value = value;
        }
    }

    public StaticWalletData() : this(Array.Empty<string>(), 1, 1, null) { }

    public StaticWalletData(string[] accounts, int chainId, int networkId, ClientMeta clientMeta)
    {
        AccountsObservable = new ObservableValue<string[]>() {Value = accounts};
        ChainIdObservable = new ObservableValue<int>() {Value = chainId};
        NetworkIdObservable = new ObservableValue<int>() {Value = networkId};
        ClientMeta = clientMeta;
    }
}
