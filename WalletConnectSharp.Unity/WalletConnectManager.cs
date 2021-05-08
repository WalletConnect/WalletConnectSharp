using System;
using UnityEngine;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Unity.Network;
using WalletConnectSharp.Unity.Utils;

namespace WalletConnectSharp.Unity
{
    [RequireComponent(typeof(NativeWebSocketTransport))]
    public class WalletConnectManager : BindableMonoBehavior
    {
        [BindComponent]
        private NativeWebSocketTransport _transport;
        
        public WalletConnect Provider { get; private set; }

        [SerializeField]
        public ClientMeta AppData;

        public override void Awake()
        {
            base.Awake();

            TransportFactory.Instance.RegisterDefaultTransport(BuildDefaultTransport);
        }

        private void Start()
        {
            Provider = new WalletConnect(AppData);
        }

        private ITransport BuildDefaultTransport(EventDelegator delegator)
        {
            _transport.AttachEventDelegator(delegator);

            return _transport;
        }
    }
}