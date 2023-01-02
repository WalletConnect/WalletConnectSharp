using System;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Crypto;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Events;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core
{
    public class Core : ICore
    {
        public static readonly string STORAGE_PREFIX = ICore.Protocol + "@" + ICore.Version + ":core:";

        private string _optName;
        public string Name
        {
            get
            {
                return $"{_optName}-core";
            }
        }

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public EventDelegator Events { get; }
        public bool Initialized { get; private set; }
        public string RelayUrl { get; }
        public string ProjectId { get; }
        public IHeartBeat HeartBeat { get; }
        public ICrypto Crypto { get; }
        public IRelayer Relayer { get; }
        public IKeyValueStorage Storage { get; }
        
        public Core(CoreOptions options = null)
        {
            if (options == null)
            {
                var storage = new InMemoryStorage();
                options = new CoreOptions()
                {
                    KeyChain = new KeyChain(storage),
                    LoggerContext = Context, //TODO Add logger
                    ProjectId = null,
                    RelayUrl = null,
                    Storage = storage
                };
            }

            if (options.Storage == null)
            {
                options.Storage = new FileSystemStorage();
            }

            if (options.KeyChain == null)
            {
                options.KeyChain = new KeyChain(options.Storage);
            }

            ProjectId = options.ProjectId;
            RelayUrl = options.RelayUrl;
            Crypto = new Crypto.Crypto(options.KeyChain);
            Storage = options.Storage;
            HeartBeat = new HeartBeat();
            _optName = options.Name;
            Events = new EventDelegator(this);
            
            Relayer = new Relayer(new RelayerOptions()
            {
                Core = this,
                LoggerContext = Context,
                ProjectId = ProjectId,
                RelayUrl = options.RelayUrl
            });
        }

        public async Task Start()
        {
            if (Initialized) return;

            await Initialize();
        }

        private async Task Initialize()
        {
            await Storage.Init();
            await Crypto.Init();
            await Relayer.Init();
            await HeartBeat.Init();
            Initialized = true;
        }
    }
}