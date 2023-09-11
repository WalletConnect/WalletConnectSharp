using WalletConnectSharp.Auth;
using WalletConnectSharp.Auth.Interfaces;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Web3Wallet.Interfaces;

public interface IWeb3WalletEngine : IWeb3WalletApi
{
    ISignClient SignClient { get; }

    IAuthClient AuthClient { get; }

    IWeb3Wallet Client { get; }

    Task Init();
}
