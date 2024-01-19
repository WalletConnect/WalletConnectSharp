using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Common;

namespace WalletConnectSharp.Auth.Interfaces;

public interface IAuthEngine : IModule
{
    IAuthClient Client { get; }
    
    IDictionary<long, PendingRequest> PendingRequests { get; }

    Task Init();

    Task<RequestUri> Request(RequestParams @params, string topic = null);

    Task Respond(Message message, string iss);

    string FormatMessage(Cacao.CacaoPayload cacao);
}
