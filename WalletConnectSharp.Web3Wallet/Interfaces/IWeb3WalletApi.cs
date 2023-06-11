using WalletConnectSharp.Auth;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;

namespace WalletConnectSharp.Web3Wallet.Interfaces;

public interface IWeb3WalletApi
{
    IDictionary<string, SessionStruct> ActiveSessions { get; }

    IDictionary<long, ProposalStruct> PendingSessionProposals { get; }

    PendingRequestStruct[] PendingSessionRequests { get; }
    
    IDictionary<long, PendingRequest> PendingAuthRequests { get; }
    
    Task Pair(string uri, bool activatePairing = false);

    Task<SessionStruct> ApproveSession(long id, Namespaces namespaces, string relayProtocol = null);

    Task<SessionStruct> ApproveSession(ProposalStruct proposal, params string[] approvedAddresses);

    Task RejectSession(long id, Error reason);

    Task RejectSession(ProposalStruct proposal, Error reason);
    
    
    Task RejectSession(ProposalStruct proposal, string reason);

    Task UpdateSession(string topic, Namespaces namespaces);

    Task ExtendSession(string topic);

    Task RespondSessionRequest<T, TR>(string topic, JsonRpcResponse<TR> response);
    
    Task EmitSessionEvent<T>(string topic, EventData<T> eventData, string chainId);

    Task DisconnectSession(string topic, Error reason);

    Task RespondAuthRequest(ResultResponse results, string iss);

    Task RespondAuthRequest(AuthErrorResponse error, string iss);

    string FormatMessage(Cacao.CacaoRequestPayload payload, string iss);
}
