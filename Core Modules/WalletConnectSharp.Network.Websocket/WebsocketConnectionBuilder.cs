using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectSharp.Network.Websocket
{
    public class WebsocketConnectionBuilder : IConnectionBuilder
    {
        public Task<IJsonRpcConnection> CreateConnection(string url)
        {
            return Task.FromResult<IJsonRpcConnection>(new WebsocketConnection(url));
        }
    }
}
