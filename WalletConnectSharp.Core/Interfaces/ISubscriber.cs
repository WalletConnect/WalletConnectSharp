using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Subscriber;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface ISubscriber : IEvents, IModule
    {
        public IReadOnlyDictionary<string, SubscriberActive> Subscriptions { get; }
        
        public ISubscriberMap TopicMap { get; }
        
        public int Length { get; }
        
        public string[] Ids { get; }
        
        public SubscriberActive[] Values { get; }
        
        public string[] Topics { get; }

        public Task Init();

        public Task<string> Subscribe(string topic, SubscribeOptions opts = null);

        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null);

        public Task<bool> IsSubscribed(string topic);
    }
}