using WalletConnectSharp.Common;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces;

public interface IAddressProvider : IModule
{
    event EventHandler<DefaultsLoadingEventArgs> DefaultsLoaded; 

    bool HasDefaultSession { get; }
    
    SessionStruct DefaultSession { get; set; }

    string DefaultNamespace { get; }

    string DefaultChainId { get; }
    
    ISession Sessions { get; }

    Caip25Address CurrentAddress(string @namespace = null, SessionStruct session = default);

    Task InitAsync();

    Task SetDefaultChainIdAsync(string chainId);

    Caip25Address[] AllAddresses(string @namespace = null, SessionStruct session = default);
}
