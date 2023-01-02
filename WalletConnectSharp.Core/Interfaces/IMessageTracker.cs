using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;

namespace WalletConnectSharp.Core.Interfaces
{
    public sealed class MessageRecord : Dictionary<string, string>
    {
    }

    public interface IMessageTracker : IModule
    {
        public Dictionary<string, MessageRecord> Messages { get; }

        public Task Init();

        public Task<string> Set(string topic, string message);

        public Task<MessageRecord> Get(string topic);

        public bool Has(string topic, string message);

        public Task Delete(string topic);
    }
}