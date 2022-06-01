using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nethereum.RLP;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;
using WalletConnectSharp.Desktop.Network;
using HexByteConvertorExtensions = Nethereum.Hex.HexConvertors.Extensions.HexByteConvertorExtensions;

namespace WalletConnectSharp.Desktop
{
    public class WalletConnect : WalletConnectSession
    {
        static WalletConnect()
        {
            TransportFactory.Instance.RegisterDefaultTransport((eventDelegator) => new WebsocketTransport(eventDelegator));
        }
        
        public WalletConnect(ClientMeta clientMeta, string bridgeUrl = null, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null) : base(clientMeta, bridgeUrl, transport, cipher, chainId, eventDelegator)
        {
        }

        public WalletConnect(SavedSession savedSession, ITransport transport = null, ICipher cipher = null,
            EventDelegator eventDelegator = null) : base(savedSession, transport, cipher, eventDelegator)
        {
        }
    }
}