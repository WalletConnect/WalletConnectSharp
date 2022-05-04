using System;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using Websocket.Client;

namespace WalletConnectSharp.Desktop.Network
{
    public class WebsocketTransport : ITransport
    {
        private WebsocketClient client;
        private EventDelegator _eventDelegator;

        public WebsocketTransport(EventDelegator eventDelegator)
        {
            this._eventDelegator = eventDelegator;
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
        }

        public bool Connected
        {
            get
            {
                return client != null && client.NativeClient != null && client.NativeClient.State == WebSocketState.Open;
            }
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public string URL { get; private set; }

        public async Task Open(string url, bool clearSubscriptions = true)
        {
            if (url.StartsWith("https"))
                url = url.Replace("https", "wss");
            else if (url.StartsWith("http"))
                url = url.Replace("http", "ws");
            
            if (client != null)
                return;

            this.URL = url;
            
            client = new WebsocketClient(new Uri(url));
            
            client.MessageReceived.ObserveOn(TaskPoolScheduler.Default).Subscribe(OnMessageReceived);
            client.DisconnectionHappened.ObserveOn(TaskPoolScheduler.Default).Subscribe(delegate(DisconnectionInfo info) { client.Reconnect(); });

            //TODO Log this
            /*client.ReconnectionHappened.Subscribe(delegate(ReconnectionInfo info)
            {
                Console.WriteLine(info.Type);
            });*/

            await client.Start();
        }
        
        private async void OnMessageReceived(ResponseMessage responseMessage)
        {
            var json = responseMessage.Text;

            var msg = JsonConvert.DeserializeObject<NetworkMessage>(json);

            await SendMessage(new NetworkMessage()
            {
                Payload = "",
                Type = "ack",
                Silent = true,
                Topic = msg.Topic
            });
            
            
            if (this.MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(msg, this));
        }

        public async Task Close()
        {
            await client.Stop(WebSocketCloseStatus.NormalClosure, "");
        }

        public async Task SendMessage(NetworkMessage message)
        {
            var finalJson = JsonConvert.SerializeObject(message);

            await this.client.SendInstant(finalJson);
        }

        public async Task Subscribe(string topic)
        {
            await SendMessage(new NetworkMessage()
            {
                Payload = "",
                Type = "sub",
                Silent = true,
                Topic = topic
            });
        }

        public async Task Subscribe<T>(string topic, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            await Subscribe(topic);

            _eventDelegator.ListenFor(topic, callback);
        }
        
        public async Task Subscribe<T>(string topic, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            await Subscribe(topic);

            _eventDelegator.ListenFor(topic, callback);
        }

        public void ClearSubscriptions()
        {
            
        }
    }
}