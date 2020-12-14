using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;

namespace Nethereum.WalletConnect.Client
{
    public class WalletConnectClient : ClientBase
    {
        public WalletConnect Provider { get; private set; }

        public WalletConnectClient(WalletConnect provider)
        {
            this.Provider = provider;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage, string route = null)
        {
            TaskCompletionSource<RpcResponseMessage> eventCompleted = new TaskCompletionSource<RpcResponseMessage>(TaskCreationOptions.None);
            
            Provider.Events.ListenFor<RpcResponseMessage>(rpcRequestMessage.Id.ToString(), (sender, args) =>
            {
                eventCompleted.SetResult(args.Response);
            });
            
            
            
            await Provider.SendRequest(rpcRequestMessage);

            return await eventCompleted.Task;
        }
    }
}