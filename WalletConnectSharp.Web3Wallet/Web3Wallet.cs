using WalletConnectSharp.Auth;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Web3Wallet.Controllers;
using WalletConnectSharp.Web3Wallet.Interfaces;
using ErrorResponse = WalletConnectSharp.Network.Models.ErrorResponse;

namespace WalletConnectSharp.Web3Wallet;

public class Web3Wallet : IWeb3Wallet
{
    public string Name { get; }
    public string Context { get; }
    public EventDelegator Events { get; }

    public IDictionary<string, SessionStruct> ActiveSessions
    {
        get
        {
            return this.Engine.ActiveSessions;
        }
    }

    public IDictionary<long, ProposalStruct> PendingSessionProposals
    {
        get
        {
            return this.Engine.PendingSessionProposals;
        }
    }

    public PendingRequestStruct[] PendingSessionRequests
    {
        get
        {
            return this.Engine.PendingSessionRequests;
        }
    }

    public IDictionary<long, PendingRequest> PendingAuthRequests
    {
        get
        {
            return this.Engine.PendingAuthRequests;
        }
    }

    public IWeb3WalletEngine Engine { get; }
    public ICore Core { get; }
    public AuthMetadata Metadata { get; }
    
    public static async Task<Web3Wallet> Init(ICore core, AuthMetadata metadata, string name = null)
    {
        var wallet = new Web3Wallet(core, metadata, name);
        await wallet.Initialize();

        return wallet;
    }
    
    private Web3Wallet(ICore core, AuthMetadata metadata, string name = null)
    {
        this.Metadata = metadata;
        this.Name = string.IsNullOrWhiteSpace(name) ? "Web3Wallet" : name;
        this.Context = $"{Name}-context";
        this.Core = core;
        
        this.Events = new EventDelegator(this);
        this.Engine = new Web3WalletEngine(this);
    }
    
    public Task Pair(string uri, bool activatePairing = false)
    {
        return this.Engine.Pair(uri, activatePairing);
    }

    public Task<SessionStruct> ApproveSession(long id, Namespaces namespaces, string relayProtocol = null)
    {
        return this.Engine.ApproveSession(id, namespaces, relayProtocol);
    }

    public Task<SessionStruct> ApproveSession(ProposalStruct proposal, params string[] approvedAddresses)
    {
        return this.Engine.ApproveSession(proposal, approvedAddresses);
    }

    public Task RejectSession(long id, ErrorResponse reason)
    {
        return this.Engine.RejectSession(id, reason);
    }

    public Task RejectSession(ProposalStruct proposal, ErrorResponse reason)
    {
        return this.Engine.RejectSession(proposal, reason);
    }

    public Task RejectSession(ProposalStruct proposal, string reason)
    {
        return this.Engine.RejectSession(proposal, reason);
    }

    public Task UpdateSession(string topic, Namespaces namespaces)
    {
        return this.Engine.UpdateSession(topic, namespaces);
    }

    public Task ExtendSession(string topic)
    {
        return this.Engine.ExtendSession(topic);
    }

    public Task RespondSessionRequest<T, TR>(string topic, JsonRpcResponse<TR> response)
    {
        return this.Engine.RespondSessionRequest<T, TR>(topic, response);
    }

    public Task EmitSessionEvent<T>(string topic, EventData<T> eventData, string chainId)
    {
        return this.Engine.EmitSessionEvent(topic, eventData, topic);
    }

    public Task DisconnectSession(string topic, ErrorResponse reason)
    {
        return this.Engine.DisconnectSession(topic, reason);
    }

    public Task RespondAuthRequest(ResultResponse results, string iss)
    {
        return this.Engine.RespondAuthRequest(results, iss);
    }

    public Task RespondAuthRequest(AuthErrorResponse error, string iss)
    {
        return this.Engine.RespondAuthRequest(error, iss);
    }

    public string FormatMessage(Cacao.CacaoRequestPayload payload, string iss)
    {
        return this.Engine.FormatMessage(payload, iss);
    }

    private Task Initialize()
    {
        return this.Engine.Init();
    }
}
