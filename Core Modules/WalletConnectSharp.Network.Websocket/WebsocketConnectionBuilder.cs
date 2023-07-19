using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectSharp.Network.Websocket
{
    public class WebsocketConnectionBuilder : IConnectionBuilder
    {
        public IJsonRpcConnection CreateConnection(string url)
        {
            return new WebsocketConnection(url);
        }
    }
}
