using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core
{
    public class WalletConnectSession : WalletConnectProtocol
    {
        
        private readonly string _handshakeTopic;
        
        private long _handshakeId;
        
        public event EventHandler<WalletConnectSession> OnSessionConnect;
        public event EventHandler<WalletConnectSession> OnSend;

        public int? NetworkId { get; private set; }
        
        public string[] Accounts { get; private set; }

        public int? ChainId { get; private set; }

        public ClientMeta ClientMetadata { get; set; }
        
        private string clientId = "";

        public string URI
        {
            get
            {
                var topicEncode = WebUtility.UrlEncode(_handshakeTopic);
                var versionEncode = WebUtility.UrlEncode(Version);
                var bridgeUrlEncode = WebUtility.UrlEncode(_bridgeUrl);
                var keyEncoded = WebUtility.UrlEncode(_key);

                return "wc:" + topicEncode + "@" + versionEncode + "?bridge=" + bridgeUrlEncode + "&key=" + keyEncoded;
            }
        }
        
        public WalletConnectSession(SavedSession savedSession, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null) : base(savedSession, transport, cipher, eventDelegator)
        {
            this.ClientMetadata = savedSession.ClientMeta;
            this.ChainId = savedSession.ChainID;
            
            clientId = savedSession.ClientID;
            
            this.Accounts = savedSession.Accounts;
                        
            this.NetworkId = savedSession.NetworkID;
        }

        public WalletConnectSession(ClientMeta clientMeta, string bridgeUrl = null, ITransport transport = null, ICipher cipher = null, int? chainId = 1, EventDelegator eventDelegator = null) : base(transport, cipher, eventDelegator)
        {
            if (clientMeta == null)
            {
                throw new ArgumentException("clientMeta cannot be null!");
            }

            if (string.IsNullOrWhiteSpace(clientMeta.Description))
            {
                throw new ArgumentException("clientMeta must include a valid Description");
            }
            
            if (string.IsNullOrWhiteSpace(clientMeta.Name))
            {
                throw new ArgumentException("clientMeta must include a valid Name");
            }
            
            if (string.IsNullOrWhiteSpace(clientMeta.URL))
            {
                throw new ArgumentException("clientMeta must include a valid URL");
            }
            
            if (clientMeta.Icons == null || clientMeta.Icons.Length == 0)
            {
                throw new ArgumentException("clientMeta must include an array of Icons the Wallet app can use. These Icons must be URLs to images. You must include at least one image URL to use");
            }
            
            if (bridgeUrl == null)
            {
                bridgeUrl = DefaultBridge.ChooseRandomBridge();
            }
            
            if (bridgeUrl.StartsWith("https"))
                bridgeUrl = bridgeUrl.Replace("https", "wss");
            else if (bridgeUrl.StartsWith("http"))
                bridgeUrl = bridgeUrl.Replace("http", "ws");
            
            this.ClientMetadata = clientMeta;
            this.ChainId = chainId;
            this._bridgeUrl = bridgeUrl;

            var topicGuid = Guid.NewGuid();

            _handshakeTopic = topicGuid.ToString();

            clientId = Guid.NewGuid().ToString();
            
            GenerateKey();
        }
        
        private void GenerateKey()
        {
            //Generate a random secret
            byte[] secret = new byte[32];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(secret);

            this._keyRaw = secret;

            //Convert hex 
            this._key = this._keyRaw.ToHex().ToLower();
        }

        public async Task<WCSessionData> ConnectSession()
        {
            if (!base.TransportConnected)
            {
                await base.SetupTransport();
            }
            
            await SubscribeAndListenToTopic(this.clientId);
            
            ListenToTopic(this._handshakeTopic);

            var result = await CreateSession();
            
            if (OnSessionConnect != null)
                OnSessionConnect(this, this);

            return result;
        }
        
        public override async Task Connect()
        {
            await base.Connect();

            await ConnectSession();
        }

        public async Task DisconnectSession(string disconnectMessage = "Session Disconnected")
        {
            var request = new WCSessionUpdate(new WCSessionData()
            {
                approved = false,
                chainId = null,
                accounts = null,
                networkId = null
            });

            await SendRequest(request);
            
            await base.Disconnect();
            
            HandleSessionDisconnect(disconnectMessage);
        }

        public override async Task Disconnect()
        {
            await DisconnectSession();
        }
        
        public async Task<string> EthSign(string address, string message, Encoding messageEncoding = null)
        {
            if (!message.IsHex())
            {
                var encoding = messageEncoding;
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }
                
                message = "0x" + encoding.GetBytes(message).ToHex();
            }
            
            var request = new EthPersonalSign(address, message);

            var response = await Send<EthPersonalSign, EthResponse>(request);

            return response.Result;
        }

        public async Task<string> EthPersonalSign(string address, string message, Encoding messageEncoding = null)
        {
            if (!message.IsHex())
            {
                var encoding = messageEncoding;
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }
                
                message = "0x" + encoding.GetBytes(message).ToHex();
            }
            
            var request = new EthPersonalSign(address, message);

            var response = await Send<EthPersonalSign, EthResponse>(request);

            return response.Result;
        }

        public async Task<string> EthSignTypedData<T>(string address, T data, EIP712Domain eip712Domain)
        {
            var request = new EthSignTypedData<T>(address, data, eip712Domain);

            var response = await Send<EthSignTypedData<T>, EthResponse>(request);

            return response.Result;
        }

        public async Task<string> EthSendTransaction(params TransactionData[] transaction)
        {
            var request = new EthSendTransaction(transaction);
            
            var response = await Send<EthSendTransaction, EthResponse>(request);

            return response.Result;
        }

        public async Task<R> Send<T, R>(T data) where T : JsonRpcRequest where R : JsonRpcResponse
        {
            TaskCompletionSource<R> eventCompleted = new TaskCompletionSource<R>(TaskCreationOptions.None);
            
            Events.ListenForResponse<R>(data.ID, (sender, @event) =>
            {
                var response = @event.Response;
                if (response.IsError)
                {
                    eventCompleted.SetException(new IOException(response.Error.Message));
                }
                else
                {
                    eventCompleted.SetResult(@event.Response);
                }
                
            });

            await SendRequest(data);

            if (OnSend != null)
            {
                OnSend(this, this);
            }

            return await eventCompleted.Task;
        }

        /// <summary>
        /// Create a new WalletConnect session with a Wallet.
        /// </summary>
        /// <returns></returns>
        private async Task<WCSessionData> CreateSession()
        {
            var data = new WcSessionRequestRequest(ClientMetadata, clientId, ChainId);

            this._handshakeId = data.ID;

            await SendRequest(data, this._handshakeTopic);

            TaskCompletionSource<WCSessionData> eventCompleted =
                new TaskCompletionSource<WCSessionData>(TaskCreationOptions.None);

            //Listen for the _handshakeId response
            //The response will be of type WCSessionRequestResponse
            Events.ListenForResponse<WCSessionRequestResponse>(this._handshakeId, HandleSessionResponse);

            //Listen for the "connect" event triggered by 'HandleSessionResponse' above
            //This will have the type WCSessionData
            Events.ListenFor<WCSessionData>("connect",
                (sender, @event) =>
                {
                    eventCompleted.TrySetResult(@event.Response);
                });
            
            //Listen for the "session_failed" event triggered by 'HandleSessionResponse' above
            //This will have the type failure reason
            Events.ListenFor<ErrorResponse>("session_failed",
                delegate(object sender, GenericEvent<ErrorResponse> @event)
                {
                    if (@event.Response.Message == "Not Approved" || @event.Response.Message == "Session Rejected")
                    {
                        eventCompleted.TrySetCanceled();
                    }
                    else
                    {
                        eventCompleted.TrySetException(
                            new IOException("WalletConnect: Session Failed: " + @event.Response.Message));
                    }
                });

            var response = await eventCompleted.Task;

            return response;
        }

        private void HandleSessionResponse(object sender, JsonRpcResponseEvent<WCSessionRequestResponse> jsonresponse)
        {
            var response = jsonresponse.Response.result;

            if (response != null && response.approved)
            {
                bool wasConnected = Connected;

                //We are now connected
                Connected = true;

                ChainId = response.chainId;

                Accounts = response.accounts;

                if (!wasConnected)
                {
                    PeerId = response.peerId;

                    ClientMetadata = response.peerMeta;

                    Events.Trigger("connect", response);
                }
                else
                {
                    Events.Trigger("session_update", response);
                }
            }
            else if (jsonresponse.Response.IsError)
            {
                HandleSessionDisconnect(jsonresponse.Response.Error.Message, "session_failed");
            }
            else
            {
                HandleSessionDisconnect("Not Approved", "session_failed");
            }
        }

        private void HandleSessionDisconnect(string msg, string topic = "disconnect")
        {
            Connected = false;

            Events.Trigger(topic, new ErrorResponse(msg));

            Transport.Close();
        }
        
        
        
        /// <summary>
        /// Creates and returns a serializable class that holds all session data required to resume later
        /// </summary>
        /// <returns></returns>
        public SavedSession SaveSession()
        {
            if (!Connected)
            {
                return null;
            }
            
            return new SavedSession(clientId, _bridgeUrl, _key, _keyRaw, PeerId, NetworkId, Accounts, ChainId, ClientMetadata);
        }

        /// <summary>
        /// Save the current session to a Stream. This function will write a GZIP Compressed JSON blob
        /// of the contents of SaveSession()
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="leaveStreamOpen">Whether to leave the stream open</param>
        /// <exception cref="IOException">If there is currently no session active, or if writing to the stream fails</exception>
        public void SaveSession(Stream stream, bool leaveStreamOpen = true)
        {
            //We'll save the current session as a GZIP compressed JSON blob
            var data = SaveSession();

            if (data == null)
            {
                throw new IOException("No session is active to save");
            }

            var json = JsonConvert.SerializeObject(data);

            byte[] encodedJson = Encoding.UTF8.GetBytes(json);
            
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress, leaveStreamOpen))
            {
                byte[] sizeEncoded = BitConverter.GetBytes(encodedJson.Length);
                
                gZipStream.Write(sizeEncoded, 0, sizeEncoded.Length);
                gZipStream.Write(encodedJson, 0, encodedJson.Length);
            }
        }

        /// <summary>
        /// Reads a GZIP Compressed JSON blob of a SavedSession object from a given Stream. This is the reverse of
        /// SaveSession(Stream)
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="leaveStreamOpen">Whether to leave the stream open</param>
        /// <exception cref="IOException">If reading from the stream fails</exception>
        /// <returns>A SavedSession object</returns>
        public static SavedSession ReadSession(Stream stream, bool leaveStreamOpen = true)
        {
            string json;
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress, leaveStreamOpen))
            {
                byte[] sizeEncoded = new byte[4];

                gZipStream.Read(sizeEncoded, 0, 4);

                int size = BitConverter.ToInt32(sizeEncoded, 0);

                byte[] jsonEncoded = new byte[size];

                gZipStream.Read(jsonEncoded, 0, size);

                json = Encoding.UTF8.GetString(jsonEncoded);
            }

            return JsonConvert.DeserializeObject<SavedSession>(json);
        }
    }
}