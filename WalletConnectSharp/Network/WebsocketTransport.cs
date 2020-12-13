using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Models;
using Websocket.Client;

namespace WalletConnectSharp.Network
{
    public class WebsocketTransport : ITransport
    {
        private WebsocketClient client;

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public async Task Open(string url)
        {
            if (client != null)
                return;
            
            client = new WebsocketClient(new Uri(url));
            
            client.MessageReceived.Subscribe(OnMessageReceived);

            await client.Start();
        }
        
        private void OnMessageReceived(ResponseMessage responseMessage)
        {
            var json = responseMessage.Text;

            var msg = JsonConvert.DeserializeObject<NetworkMessage>(json);

            SendMessage(new NetworkMessage()
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

        public void SendMessage(NetworkMessage message)
        {
            var finalJson = JsonConvert.SerializeObject(message);
            
            this.client.Send(finalJson);
        }
    }
}