using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Events;
using WalletConnectSharp.Models;
using WalletConnectSharp.Network;
using Websocket.Client;

namespace WalletConnectSharp
{
    public class WalletConnect : IDisposable
    {
        private static readonly string[] SigningMethods = new[]
        {
            "eth_sendTransaction",
            "eth_signTransaction",
            "eth_sign",
            "eth_signTypedData",
            "eth_signTypedData_v1",
            "eth_signTypedData_v2",
            "eth_signTypedData_v3",
            "eth_signTypedData_v4",
            "personal_sign",
        };

        private string clientId = "";
        private readonly string _handshakeTopic;
        private long _handshakeId;
        private const string Version = "1";
        private readonly string _bridgeUrl;
        private string _key;
        private byte[] _keyRaw;
        private string peerId;
        
        public int? ChainId { get; }

        public ClientMeta ClientMetadata { get; set; }
        
        public ITransport Transport { get; private set; }
        
        public ICipher Cipher { get; private set; }

        public WalletConnect(ClientMeta clientMeta, ITransport transport = null,
            ICipher cipher = null,
            int? chainId = 1, 
            string bridgeUrl = "https://bridge.walletconnect.org")
        {
            this.ClientMetadata = clientMeta;
            this.ChainId = chainId;
            
            var topicGuid = Guid.NewGuid();

            _handshakeTopic = topicGuid.ToString();

            clientId = Guid.NewGuid().ToString();
            
            if (transport == null)
                transport = new WebsocketTransport();

            this._bridgeUrl = bridgeUrl;
            this.Transport = transport;
            
            if (cipher == null)
                cipher = new AESCipher();

            this.Cipher = cipher;
            
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
            this._key = BitConverter.ToString(secret).Replace("-", "").ToLower();
        }

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

        private void SubscribeToInternalEvents()
        {
            Transport.ListenFor("wc_sessionRequest",
                delegate(object sender, JsonRpcRequestEvent<WcSessionRequestRequest> payload)
                {
                    Console.WriteLine(payload);
                });
        }

        public async Task<WCSessionRequestResponse> Connect()
        {
            Transport.MessageReceived += TransportOnMessageReceived;
            
            await Transport.Open(this._bridgeUrl);

            await Transport.Subscribe(this.clientId);
            
            SubscribeToInternalEvents();
            
            return await CreateSession();
        }

        private async void TransportOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string[] activeTopics = new[] {this.clientId, _handshakeTopic};

            var networkMessage = e.Message;

            if (!activeTopics.Contains(networkMessage.Topic))
                return;

            var encryptedPayload = JsonConvert.DeserializeObject<EncryptedPayload>(networkMessage.Payload);

            var json = await Cipher.DecryptWithKey(_keyRaw, encryptedPayload);

            var response = JsonConvert.DeserializeObject<JsonRpcResponse>(json);
            
            //TODO Handle this case better, how to differentiate between Response and Request Object?
            if (response.Event != null)
                Transport.Trigger(response.Event, json);
            else
            {
                var request = JsonConvert.DeserializeObject<JsonRpcRequest>(json);
                
                if (request.ID != 0)
                    Transport.Trigger("request:" + request.ID, json);
            }
        }

        private async Task<WCSessionRequestResponse> CreateSession()
        {
            var data = new WcSessionRequestRequest(ClientMetadata, clientId, ChainId);

            this._handshakeId = data.ID;
            
            await SendRequest(data, this._handshakeTopic);
            
            TaskCompletionSource<WCSessionRequestResponse> eventCompleted = new TaskCompletionSource<WCSessionRequestResponse>(TaskCreationOptions.None);

            //Subscribe and provide a delegate for the event
            Transport.ListenFor(this._handshakeId.ToString(), 
                delegate(object sender, JsonRpcResponseEvent<WCSessionRequestResponse> eventResponse)
                {
                    eventCompleted.SetResult(eventResponse.Response);
                });
            
            var response = await eventCompleted.Task;

            return response;
        }

        private async Task SendRequest(JsonRpcRequest requestObject, string sendingTopic = null, bool? forcePushNotification = null)
        {
            string json = JsonConvert.SerializeObject(requestObject);

            var encrypted = await Cipher.EncryptWithKey(_keyRaw, json);

            if (sendingTopic == null)
                sendingTopic = peerId;

            bool silent;
            if (forcePushNotification != null)
            {
                silent = (bool) !forcePushNotification;
            }
            else
            {
                silent = requestObject.Method.StartsWith("wc_") || !SigningMethods.Contains(requestObject.Method);
            }

            var message = new NetworkMessage()
            {
                Payload = JsonConvert.SerializeObject(encrypted),
                Silent = silent,
                Topic = sendingTopic,
                Type = "pub"
            };
            
            await this.Transport.SendMessage(message);
        }

        public void Dispose()
        {
            if (Transport != null)
            {
                Transport.Dispose();
                Transport = null;
            }
        }
    }
}