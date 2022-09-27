using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Models;

public interface IWalletData
{
    ObservableValue<string[]> AccountsObservable { get; }
    
    ObservableValue<int> ChainIdObservable { get; }
    
    ObservableValue<int> NetworkIdObservable { get; }
    
    ClientMeta ClientMeta { get; set; }
}
