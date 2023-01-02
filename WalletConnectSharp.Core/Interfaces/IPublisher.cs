using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IPublisher : IEvents, IModule
    {
        public IRelayer Relayer { get; }
        
        public Task Publish(string topic, string message, PublishOptions opts = null);
    }
}