using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Network;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IRelayer : IEvents, IModule
    {
        public const string Protocol = RelayConstants.Protocol;
        public const int Version = RelayConstants.Version;
        
        public ICore Core { get; }
        
        //TODO Add logger

        public ISubscriber Subscriber { get; }
        
        public IPublisher Publisher { get; }
        
        public IMessageTracker Messages { get; }
        
        public IJsonRpcProvider Provider { get; }
        
        public bool Connected { get; }
        
        public bool Connecting { get; }

        public Task Init();

        public Task Publish(string topic, string message, PublishOptions opts = null);

        public Task<string> Subscribe(string topic, SubscribeOptions opts = null);

        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null);
    }
}