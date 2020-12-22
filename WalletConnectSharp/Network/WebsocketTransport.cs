using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Request;
using WalletConnectSharp.Events.Response;
using WalletConnectSharp.Models;
using Websocket.Client;
using Websocket.Client.Models;

namespace WalletConnectSharp.Network
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

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public async Task Open(string url)
        {
            if (url.StartsWith("https"))
                url = url.Replace("https", "wss");
            else if (url.StartsWith("http"))
                url = url.Replace("http", "ws");
            
            if (client != null)
                return;
            
            client = new WebsocketClient(new Uri(url));
            
            client.MessageReceived.Subscribe(OnMessageReceived);
            client.DisconnectionHappened.Subscribe(delegate(DisconnectionInfo info) { client.Reconnect(); });

            client.ReconnectionHappened.Subscribe(delegate(ReconnectionInfo info)
            {
                Console.WriteLine(info.Type);
            });

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
    }
}