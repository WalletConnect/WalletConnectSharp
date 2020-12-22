using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;

namespace WalletConnectSharp.Client.Nethereum
{
    public class WalletConnectClient : ClientBase
    {
        
        public WalletConnectSharp.WalletConnect Provider { get; }

        public WalletConnectClient(WalletConnectSharp.WalletConnect provider)
        {
            this.Provider = provider;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage, string route = null)
        {
            TaskCompletionSource<RpcResponseMessage> eventCompleted = new TaskCompletionSource<RpcResponseMessage>(TaskCreationOptions.None);
            
            Provider.Events.ListenForResponse<RpcResponseMessage>(rpcRequestMessage.Id, (sender, args) =>
            {
                eventCompleted.SetResult(args.Response);
            });
            
            await Provider.SendRequest(rpcRequestMessage);

            return await eventCompleted.Task;
        }
    }
}