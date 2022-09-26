using System.Net;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core;

public class WalletConnectProvider : WalletConnectProtocol
{
    private string _handshakeTopic;

    private long _handshakeId;

    public event EventHandler<WalletConnectProtocol> OnProviderConnect;

    public int? NetworkId { get; private set; }

    public string[] Accounts { get; private set; }

    public int ChainId { get; private set; }

    public ClientMeta ClientMetadata { get; set; }
    public ClientMeta PeerMetadata { get; set; }
    
    public bool ReadyForUserPrompt { get; private set; }

    private string clientId = "";

    public string URI
    {
        get;
        private set;
    }

    public WalletConnectProvider(SavedSession savedSession, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null) : base(savedSession, transport, cipher, eventDelegator)
    {
        this.ClientMetadata = savedSession.DappMeta;
        this.WalletMetadata = savedSession.WalletMeta;
        this.ChainId = savedSession.ChainID;

        clientId = savedSession.ClientID;

        this.Accounts = savedSession.Accounts;

        this.NetworkId = savedSession.NetworkID;
    }

    public WalletConnectProvider(string url, ClientMeta walletMeta, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null) : base(transport, cipher, eventDelegator)
    {
        ClientMetadata = walletMeta;
        this.ChainId = chainId;
        this.URI = url;

        this.ParseUrl();
    }

    protected void ParseUrl()
    {
        /*
         *  var topicEncode = WebUtility.UrlEncode(_handshakeTopic);
            var versionEncode = WebUtility.UrlEncode(Version);
            var bridgeUrlEncode = WebUtility.UrlEncode(_bridgeUrl);
            var keyEncoded = WebUtility.UrlEncode(_key);

            return "wc:" + topicEncode + "@" + versionEncode + "?bridge=" + bridgeUrlEncode + "&key=" + keyEncoded;
         */

        if (!this.URI.StartsWith("wc"))
            return;

        //TODO Figure out a better way to parse this

        // topicEncode + "@" + versionEncode + "?bridge=" + bridgeUrlEncode + "&key=" + keyEncoded
        var data = this.URI.Split(':')[1];

        _handshakeTopic = WebUtility.UrlDecode(data.Split('@')[0]);

        // versionEncode + "?bridge=" + bridgeUrlEncode + "&key=" + keyEncoded
        data = data.Split('@')[1];

        Version = WebUtility.UrlDecode(data.Split('?')[0]);

        //bridge=" + bridgeUrlEncode + "&key=" + keyEncoded
        data = data.Split('?')[1];
        
        var parameters = data.Split('&');

        foreach (var parm in parameters)
        {
            var parts = parm.Split('=');
            var name = parts[0];
            var value = parts[1];

            switch (name.ToLower())
            {
                case "bridge":
                    base._bridgeUrl = WebUtility.UrlDecode(value);
                    break;
                case "key":
                    base._key = WebUtility.UrlDecode(value);
                    base._keyRaw = base._key.HexToByteArray();
                    break;
            }
        }
    }

    public async Task ConnectSession()
    {
        EnsureNotDisconnected();

        Connecting = true;
        try
        {
            clientId = Guid.NewGuid().ToString();
            
            if (!base.TransportConnected)
            {
                await base.SetupTransport();
            }
            
            if (_key == null)
                ParseUrl();

            TaskCompletionSource<bool> sessionRequestTask = new TaskCompletionSource<bool>();
            Events.ListenFor(WalletConnectStates.SessionRequest,
                delegate(object sender, JsonRpcRequestEvent<WcSessionRequest> @event)
                {
                    _handshakeId = @event.Response.ID;
                    
                    UpdateSession(@event.Response.parameters[0]);
                    
                    sessionRequestTask.SetResult(true);
                });
            
            await SubscribeAndListenToTopic(this._handshakeTopic);

            await sessionRequestTask.Task;
            
            ReadyForUserPrompt = true;
        }
        catch (Exception e)
        {
            
        }
    }

    private void UpdateSession(WcSessionRequest.WcSessionRequestRequestParams request)
    {
        PeerId = request.peerId;
        ChainId = request.chainId;
        PeerMetadata = request.peerMeta;
    }

    public override async Task Connect()
    {
        EnsureNotDisconnected();

        await base.Connect();

        await ConnectSession();
    }

    public Task AcceptRequest()
    {
        return ResponseToRequest(true);
    }

    public Task RejectRequest()
    {
        return ResponseToRequest(false);
    }

    public async Task ResponseToRequest(bool approved)
    {
        if (!ReadyForUserPrompt) throw new IOException("No session request to accept");

        WCSessionRequestResponse response = new WCSessionRequestResponse() {result = new WCSessionData()
        {
            // TODO Where to grab accounts from?
            accounts = new string[]
            {
                "test"
            },
            approved = true,
            chainId = ChainId,
            networkId = NetworkId,
            peerId = clientId,
            peerMeta = ClientMetadata
        }};

        response.ID = _handshakeId;

        await SendRequest(response);
    }
}
