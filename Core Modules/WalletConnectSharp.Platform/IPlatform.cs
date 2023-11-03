using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectSharp.Platform;

public interface IPlatform
{
    IConnectionBuilder ConnectionBuilder { get; set; }
    
    /// <summary>
    /// The <see cref="IRelayUrlBuilder"/> module to use for building the relay url.
    /// If this is null, then the default <see cref="RelayUrlBuilder"/> module will be used by Core.
    /// </summary>
    public IRelayUrlBuilder RelayUrlBuilder { get; set; }
    
    public (string name, string version) GetOsInfo();

    public (string name, string version) GetSdkInfo();

    Task OpenUrl(string url);
}
