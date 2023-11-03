using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectSharp.Platform;

public interface IPlatform
{
    IConnectionBuilder ConnectionBuilder { get; }

    Task OpenUrl(string url);
}
