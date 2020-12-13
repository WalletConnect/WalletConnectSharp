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
        private const string Version = "1";
        private readonly string _bridgeUrl;
        private string _key;
        private byte[] _keyRaw;
        private string peerId;
        
        public EventManager Events { get; private set; }
        
        public int? ChainId { get; }

        public ClientMeta ClientMetadata { get; set; }
        
        public ITransport Transport { get; private set; }
        
        public ICipher Cipher { get; private set; }

        public WalletConnect(ClientMeta clientMeta, ITransport transport = null,
            ICipher cipher = null,
            int? chainId = null, 
            string bridgeUrl = "https://bridge.walletconnect.org")
        {
            Events = new EventManager();
            
            this.ClientMetadata = clientMeta;
            this.ChainId = chainId;
            
            var topicGuid = Guid.NewGuid();

            _handshakeTopic = topicGuid.ToString();

            clientId = Guid.NewGuid().ToString();

            if (bridgeUrl.StartsWith("https"))
                bridgeUrl = bridgeUrl.Replace("https", "wss");
            else if (bridgeUrl.StartsWith("http"))
                bridgeUrl = bridgeUrl.Replace("http", "ws");
            
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

        public async Task<WCSessionRequestResponse> Connect()
        {
            Transport.MessageReceived += TransportOnMessageReceived;
            
            await Transport.Open(this._bridgeUrl);
            
            CreateSession();
            
            TaskCompletionSource<WCSessionRequestResponse> eventCompleted = new TaskCompletionSource<WCSessionRequestResponse>(TaskCreationOptions.None);

            Events.SessionResponse += delegate(object sender, JsonRpcResponseEvent<WCSessionRequestResponse> eventResponse)
            {
                eventCompleted.SetResult(eventResponse.Response);
            };
            
            var response = await eventCompleted.Task;

            return response;
        }

        private async void TransportOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string[] activeTopics = new[] {this.clientId, _handshakeTopic};

            var networkMessage = e.Message;

            if (!activeTopics.Contains(networkMessage.Topic))
                return;

            var encryptedPayload = JsonConvert.DeserializeObject<EncryptedPayload>(networkMessage.Payload);

            var json = await Cipher.DecryptWithKey(_keyRaw, encryptedPayload);

            var jsonResponse = JsonConvert.DeserializeObject<JsonRpcResponse>(json);
            
            Events.ExecuteEvent(jsonResponse);
        }

        private void CreateSession()
        {
            var data = new WcSessionRequestRequest(ClientMetadata, clientId, ChainId);
            
            SendRequest(data, this._handshakeTopic);
        }

        private async void SendRequest(JsonRpcRequest requestObject, string sendingTopic = null, bool? forcePushNotification = null)
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
            
            this.Transport.SendMessage(message);
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