using WalletConnectSharp.Auth.Controllers;
using WalletConnectSharp.Auth.Interfaces;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events;

namespace WalletConnectSharp.Auth;

public class AuthClient : IAuthClient
{
    public const string AUTH_CLIENT_PROTOCOL = AuthEngine.AUTH_CLIENT_PROTOCOL;
    public const int AUTH_CLIENT_VERSION = AuthEngine.AUTH_CLIENT_VERSION;
    public const string AUTH_CLIENT_DEFAULT_NAME = "authClient";
    public static readonly string AUTH_CLIENT_STORAGE_PREFIX = AuthEngine.AUTH_CLIENT_STORAGE_PREFIX;

    public string Name
    {
        get
        {
            return AUTH_CLIENT_DEFAULT_NAME;
        }
    }

    public string Context
    {
        get
        {
            return $"{Name}-{Version}-context";
        }
    }

    public EventDelegator Events { get; }

    public string Protocol
    {
        get
        {
            return AUTH_CLIENT_PROTOCOL;
        }
    }

    public int Version
    {
        get
        {
            return AUTH_CLIENT_VERSION;
        }
    }

    public event EventHandler<AuthRequest> AuthRequested;
    public event EventHandler<AuthResponse> AuthResponded;
    public event EventHandler<AuthErrorResponse> AuthError;
    public ICore Core { get; set; }
    public Metadata Metadata { get; set; }
    public string ProjectId { get; set; }
    public IStore<string, AuthData> AuthKeys { get; set; }
    public IStore<string, PairingData> PairingTopics { get; set; }
    public IStore<long, Message> Requests { get; set; }
    public IAuthEngine Engine { get; }
    public AuthOptions Options { get; }

    public IDictionary<long, PendingRequest> PendingRequests
    {
        get
        {
            return Engine.PendingRequests;
        }
    }

    public static async Task<IAuthClient> Init(AuthOptions options)
    {
        var client = new AuthClient(options);
        await client.Initialize();
        return client;
    }

    private async Task Initialize()
    {
        await this.Core.Start();
        await this.AuthKeys.Init();
        await this.Requests.Init();
        await this.PairingTopics.Init();
        this.Engine.Init();
    }

    private AuthClient(AuthOptions options)
    {
        Options = options;
        Metadata = options.Metadata;
        ProjectId = options.ProjectId;
        Core = options.Core ?? new Core.Core(options);

        AuthKeys = new Store<string, AuthData>(Core, "authKeys", AUTH_CLIENT_STORAGE_PREFIX);
        PairingTopics = new Store<string, PairingData>(Core, "pairingTopics", AUTH_CLIENT_STORAGE_PREFIX);
        Requests = new Store<long, Message>(Core, "requests", AUTH_CLIENT_STORAGE_PREFIX);

        Engine = new AuthEngine(this);
    }
    
    public Task<RequestUri> Request(RequestParams @params, string topic = null)
    {
        return this.Engine.Request(@params, topic);
    }

    public Task Respond(Message message, string iss)
    {
        return this.Engine.Respond(message, iss);
    }

    public string FormatMessage(Cacao.CacaoPayload cacao, string iss)
    {
        return this.Engine.FormatMessage(cacao, iss);
    }

    bool IAuthClient.OnAuthRequest(AuthRequest request)
    {
        if (AuthRequested != null)
        {
            AuthRequested(this, request);
            return true;
        }

        return false;
    }

    bool IAuthClient.OnAuthResponse(AuthErrorResponse errorResponse)
    {
        if (AuthError != null)
        {
            AuthError(this, errorResponse);
            return true;
        }

        return false;
    }

    bool IAuthClient.OnAuthResponse(AuthResponse response)
    {
        if (AuthResponded != null)
        {
            AuthResponded(this, response);
            return true;
        }

        return false;
    }
}
