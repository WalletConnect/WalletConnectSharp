using System;
using System.Threading.Tasks;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    public interface IEngine : IEngineTasks
    {
        ISignClient Client { get; }

        Task Init();

        void HandleMessageType<T, TR>(Func<string, JsonRpcRequest<T>, Task> requestCallback,
            Func<string, JsonRpcResponse<TR>, Task> responseCallback);

        TypedEventHandler<T, TR> SessionRequestEvents<T, TR>();
    }
}