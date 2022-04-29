using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using WalletConnectSharp.Core;

namespace WalletConnectSharp.NEthereum.Client
{
    public class WalletConnectClient : ClientBase
    {
        private long _id;
        public WalletConnectSession Session { get; }

        public WalletConnectClient(WalletConnectSession provider)
        {
            this.Session = provider;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage message, string route = null)
        {
            _id += 1;
            var mapParameters = message.RawParameters as Dictionary<string, object>;
            var arrayParameters = message.RawParameters as object[];
            var rawParameters = message.RawParameters;

            RpcRequestMessage rpcRequestMessage;
            if (mapParameters != null) 
                rpcRequestMessage = new RpcRequestMessage(_id, message.Method, mapParameters);
            else if (arrayParameters != null)
                rpcRequestMessage = new RpcRequestMessage(_id, message.Method, arrayParameters);
            else
                rpcRequestMessage = new RpcRequestMessage(_id, message.Method, rawParameters);

            TaskCompletionSource<RpcResponseMessage> eventCompleted = new TaskCompletionSource<RpcResponseMessage>(TaskCreationOptions.None);
            
            Session.Events.ListenForGenericResponse<RpcResponseMessage>(rpcRequestMessage.Id, (sender, args) =>
            {
                eventCompleted.SetResult(args.Response);
            });
            
            await Session.SendRequest(rpcRequestMessage);

            return await eventCompleted.Task;
        }
    }
}