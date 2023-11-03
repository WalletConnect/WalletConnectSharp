using WalletConnectSharp.Network.Interfaces;
using WalletConnectSharp.Network.Websocket;

namespace WalletConnectSharp.Platform;

public class DefaultPlatform : IPlatform
{
    public IConnectionBuilder ConnectionBuilder { get; }

    public DefaultPlatform()
    {
        ConnectionBuilder = new WebsocketConnectionBuilder();
    }
    
    public Task OpenUrl(string url)
    {
        throw new NotImplementedException("Not supported when using DefaultPlatform");
    }
}
