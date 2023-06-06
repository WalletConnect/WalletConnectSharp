using WalletConnectSharp.Auth;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Web3Wallet.Interfaces;

public interface IWeb3Wallet : IModule, IEvents, IWeb3WalletApi
{
    IWeb3WalletEngine Engine { get; }
    
    ICore Core { get; }
    
    AuthMetadata Metadata { get; }
}
