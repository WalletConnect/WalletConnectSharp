using System.Net;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core;

public class WalletConnectProvider : WalletConnectProtocol
{
    private string _handshakeTopic;

    private long _handshakeId;

    public event EventHandler<WalletConnectProtocol> OnProviderConnect;

    public event EventHandler<EventArgWithResponse<WalletAddEthChain, EthResponse>> WalletAddEthChain;
    public event EventHandler<EventArgWithResponse<WalletSwitchEthChain, EthResponse>> WalletSwitchEthChain;
    public event EventHandler<EventArgWithResponse<EthSign, EthResponse>> EthSign;
    public event EventHandler<EventArgWithResponse<EthPersonalSign, EthResponse>> EthPersonalSign;

    public override ClientMeta WalletMetadata
    {
        get
        {
            return WalletData.ClientMeta;
        }
        set
        {
            WalletData.ClientMeta = value;
        }
    }

    public bool ReadyForUserPrompt { get; private set; }
    
    public IWalletData WalletData { get; private set; }

    public string[] Accounts
    {
        get
        {
            return WalletData.AccountsObservable.Value;
        }
        set
        {
            WalletData.AccountsObservable.Value = value;
        }
    }

    public int ChainId
    {
        get
        {
            return WalletData.ChainIdObservable.Value;
        }
        set
        {
            WalletData.ChainIdObservable.Value = value;
        }
    }

    public int NetworkId
    {
        get
        {
            return WalletData.NetworkIdObservable.Value;
        }
        set
        {
            WalletData.NetworkIdObservable.Value = value;
        }
    }

    private string clientId = "";

    public string URI
    {
        get;
        private set;
    }

    public WalletConnectProvider(SavedSession savedSession, IWalletData walletData, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null) : base(savedSession, transport, cipher, eventDelegator)
    {
        this.DappMetadata = savedSession.DappMeta;
        this.WalletMetadata = savedSession.WalletMeta;
        this.WalletData = walletData;
        
        clientId = savedSession.ClientID;
    }

    public WalletConnectProvider(string url, IWalletData walletData, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null) : base(transport, cipher, eventDelegator)
    {
        this.URI = url;

        this.WalletData = walletData;

        this.ParseUrl();
    }

    public EventHandler<EventArgWithResponse<EthSignTypedData<T>, EthResponse>> EthSignTypedData<T>()
    {
        
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
        DappMetadata = request.peerMeta;
    }

    public override async Task Connect()
    {
        EnsureNotDisconnected();

        await base.Connect();

        await ConnectSession();
    }

    public async Task AcceptRequest()
    {
        await ResponseToRequest(true);
        
        WalletData.AccountsObservable.OnValueChanged += AccountsObservableOnOnValueChanged;
        WalletData.ChainIdObservable.OnValueChanged += ChainIdOrNetworkObservableOnOnValueChanged;
        WalletData.NetworkIdObservable.OnValueChanged += ChainIdOrNetworkObservableOnOnValueChanged;
    }

    private async void ChainIdOrNetworkObservableOnOnValueChanged(object sender, int e)
    {
        await SendSessionUpdate();
    }

    private async void AccountsObservableOnOnValueChanged(object sender, string[] e)
    {
        await SendSessionUpdate();
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
            accounts = WalletData.AccountsObservable.Value,
            chainId = WalletData.ChainIdObservable.Value,
            networkId = WalletData.NetworkIdObservable.Value,
            
            approved = approved,
            peerId = clientId,
            peerMeta = WalletMetadata
        }};

        response.ID = _handshakeId;

        await SendRequest(response);
    }

    public async Task SendSessionUpdate(bool disconnect = false)
    {
        if (!SessionConnected) throw new IOException("No session to update");

        WCSessionUpdate request = new WCSessionUpdate(new WCSessionData()
        {
            accounts = WalletData.AccountsObservable.Value,
            chainId = WalletData.ChainIdObservable.Value,
            networkId = WalletData.NetworkIdObservable.Value,
            approved = disconnect,
        });

        await SendRequest(request);
    }
    
    
}
