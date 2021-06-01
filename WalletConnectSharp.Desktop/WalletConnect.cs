using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Desktop.Network;

namespace WalletConnectSharp.Desktop
{
    public class WalletConnect : WalletConnectProtocol
    {
        static WalletConnect()
        {
            TransportFactory.Instance.RegisterDefaultTransport((eventDelegator) => new WebsocketTransport(eventDelegator));
        }
        
        public WalletConnect(ClientMeta clientMeta, ITransport transport = null, ICipher cipher = null, int? chainId = null, string bridgeUrl = "https://bridge.walletconnect.org", EventDelegator eventDelegator = null) : base(clientMeta, transport, cipher, chainId, bridgeUrl, eventDelegator)
        {
        }
    }
}