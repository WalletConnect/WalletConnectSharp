using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Auth.Models.Engine;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Auth.Interfaces;

public interface IAuthClient : IModule, IEvents
{
    string Protocol { get; }
    int Version { get; }

    event EventHandler<AuthRequest> AuthRequested;
    event EventHandler<AuthResponse> AuthResponded;
    event EventHandler<AuthErrorResponse> AuthError;

    ICore Core { get; set; }
    Metadata Metadata { get; set; }
    string ProjectId { get; set; }
    IStore<string, AuthData> AuthKeys { get; set; }
    IStore<string, PairingData> PairingTopics { get; set; }
    IStore<long, Message> Requests { get; set; }

    IAuthEngine Engine { get; }
    
    AuthOptions Options { get; }
    
    IDictionary<long, PendingRequest> PendingRequests { get; }
    
    Task<RequestUri> Request(RequestParams @params, string topic = null);

    Task Respond(Message message, string iss);

    Task<IJsonRpcHistory<WcAuthRequest, Cacao>> AuthHistory();

    string FormatMessage(Cacao.CacaoPayload cacao);

    
    string FormatMessage(Cacao.CacaoRequestPayload cacao, string iss);
    
    internal bool OnAuthRequest(AuthRequest request);

    internal bool OnAuthResponse(AuthErrorResponse errorResponse);

    internal bool OnAuthResponse(AuthResponse response);
}
