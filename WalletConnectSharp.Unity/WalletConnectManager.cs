using System.Collections;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity.Network;
using WalletConnectSharp.Unity.Utils;

namespace WalletConnectSharp.Unity
{
    [RequireComponent(typeof(NativeWebSocketTransport))]
    public class WalletConnectManager : BindableMonoBehavior
    {
        [BindComponent]
        private NativeWebSocketTransport _transport;

        private static WalletConnectManager _instance;

        public static WalletConnectManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public string ConnectURL
        {
            get
            {
                return Provider.URI;
            }
        }

        public bool dontDestroyOnLoad;
        
        public WalletConnectProtocol Provider { get; private set; }

        [SerializeField]
        public ClientMeta AppData;

        public override void Awake()
        {
            if (dontDestroyOnLoad)
            {
                if (_instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                DontDestroyOnLoad(gameObject);
            }
            
            _instance = this;
            
            base.Awake();
            
            Provider = new WalletConnectProtocol(AppData, _transport);
        }

        public void WaitForWalletConnection(UnityAction<WCSessionData> onConnected)
        {
            StartCoroutine(ConnectAsync(onConnected));
        }

        private IEnumerator ConnectAsync(UnityAction<WCSessionData> onConnected)
        {
            var coroutineInstruction = new WaitForTaskResult<WCSessionData>(Provider.Connect());
            yield return coroutineInstruction;

            var task = coroutineInstruction.Source;

            if (task.Exception != null)
            {
                throw task.Exception;
            }

            onConnected(task.Result);
        }
    }
}