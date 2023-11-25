using WalletConnectSharp.Common;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces;

public interface IAddressProvider : IModule
{
    event EventHandler<DefaultsLoadingEventArgs> DefaultsLoaded; 

    bool HasDefaultSession { get; }
    
    SessionStruct DefaultSession { get; set; }
    
    string DefaultNamespace { get; set; }
    
    string DefaultChain { get; set; }
    
    ISession Sessions { get; }
    
    Caip25Address CurrentAddress( string chain = null, SessionStruct session = default);

    Task Init();

    Caip25Address[] AllAddresses(string chain = null, SessionStruct session = default);
}
