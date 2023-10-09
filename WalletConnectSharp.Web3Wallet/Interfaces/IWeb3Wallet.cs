using EventEmitter.NET;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Web3Wallet.Interfaces;

public interface IWeb3Wallet : IModule, IEvents, IWeb3WalletApi
{
    IWeb3WalletEngine Engine { get; }
    
    ICore Core { get; }
    
    Metadata Metadata { get; }
}
