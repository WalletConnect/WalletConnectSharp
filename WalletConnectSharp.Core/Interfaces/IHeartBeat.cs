using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IHeartBeat : IEvents, IModule
    {
        public int Interval { get; }

        public Task Init();
    }
}