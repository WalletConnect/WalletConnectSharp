using WalletConnectSharp.Common;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Interfaces;
using WalletConnectSharp.Network.Websocket;

namespace WalletConnectSharp.Platform;

public class DefaultPlatform : IPlatform
{
    public IConnectionBuilder ConnectionBuilder { get; set; }
    public IRelayUrlBuilder RelayUrlBuilder { get; set; }

    public DefaultPlatform()
    {
        ConnectionBuilder = new WebsocketConnectionBuilder();
        RelayUrlBuilder = new RelayUrlBuilder();
    }
    
    public virtual (string name, string version) GetOsInfo()
    {
        var name = Environment.OSVersion.Platform.ToString().ToLowerInvariant();
        var version = Environment.OSVersion.Version.ToString().ToLowerInvariant();
        return (name, version);
    }

    public virtual (string name, string version) GetSdkInfo()
    {
        return ("csharp", SDKConstants.SDK_VERSION);
    }
    
    public virtual Task OpenUrl(string url)
    {
        throw new NotImplementedException("Not supported when using DefaultPlatform");
    }
}
