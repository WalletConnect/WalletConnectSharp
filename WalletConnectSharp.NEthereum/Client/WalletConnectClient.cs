using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.NEthereum.Client;

public class WalletConnectClient : ClientBase
{
    public WalletConnectSession Session { get; }

    public WalletConnectClient(WalletConnectSession provider)
    {
        this.Session = provider;
    }

    protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage message, string route = null)
    {
        long id = RpcPayloadId.Generate();
        var mapParameters = message.RawParameters as Dictionary<string, object>;
        var arrayParameters = message.RawParameters as object[];
        var rawParameters = message.RawParameters;

        RpcRequestMessage rpcRequestMessage;
        if (mapParameters != null)
            rpcRequestMessage = new RpcRequestMessage(id, message.Method, mapParameters);
        else if (arrayParameters != null)
            rpcRequestMessage = new RpcRequestMessage(id, message.Method, arrayParameters);
        else
            rpcRequestMessage = new RpcRequestMessage(id, message.Method, rawParameters);

        TaskCompletionSource<RpcResponseMessage> eventCompleted = new TaskCompletionSource<RpcResponseMessage>(TaskCreationOptions.None);

        Session.Events.ListenForGenericResponse<RpcResponseMessage>(rpcRequestMessage.Id, (sender, args) =>
        {
            eventCompleted.SetResult(args.Response);
        });

        await Session.SendRequest(rpcRequestMessage);

        return await eventCompleted.Task;
    }
}
