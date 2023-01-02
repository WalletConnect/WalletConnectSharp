using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface ICore : IModule, IEvents
    {
        public const string Protocol = "wc";
        public const int Version = 2;
        
        public string RelayUrl { get; }
        
        public string ProjectId { get; }
        
        //TODO Add logger
        
        public IHeartBeat HeartBeat { get; }
        
        public ICrypto Crypto { get; }
        
        public IRelayer Relayer { get; }
        
        public IKeyValueStorage Storage { get; }

        public Task Start();
    }
}