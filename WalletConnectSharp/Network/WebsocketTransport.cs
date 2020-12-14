using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Events;
using WalletConnectSharp.Models;
using Websocket.Client;
using Websocket.Client.Models;

namespace WalletConnectSharp.Network
{
    public class WebsocketTransport : ITransport
    {
        private WebsocketClient client;
        
        private Dictionary<string, List<IEventProvider>> Listeners = new Dictionary<string, List<IEventProvider>>();

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
            client.DisconnectionHappened.Subscribe(delegate(DisconnectionInfo info)
            {
                Console.WriteLine(info.Type);
            });

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

            ListenFor(topic, callback);
        }
        
        public async Task Subscribe<T>(string topic, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            await Subscribe(topic);

            ListenFor(topic, callback);
        }

        public void ListenFor<T>(string eventId, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            long _id;
            bool isNumber = long.TryParse(eventId, out _id);

            if (isNumber)
                eventId = "response:" + _id;
            
            JsonRpcResponseEventManager<T>.Instance.EventTrigger += callback;

            List<IEventProvider> listProvider;
            if (!Listeners.ContainsKey(eventId))
            {
                listProvider = new List<IEventProvider>();
                Listeners.Add(eventId, listProvider);
            }
            else
            {
                listProvider = Listeners[eventId];
            }
            
            listProvider.Add(EventFactory.Instance.ProviderFor<T>());
        }

        public void ListenFor<T>(string eventId, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            JsonRpcRequestEventManager<T>.Instance.EventTrigger += callback;

            List<IEventProvider> listProvider;
            if (!Listeners.ContainsKey(eventId))
            {
                listProvider = new List<IEventProvider>();
                Listeners.Add(eventId, listProvider);
            }
            else
            {
                listProvider = Listeners[eventId];
            }
            
            listProvider.Add(EventFactory.Instance.ProviderFor<T>());
        }


        public void Trigger(string topic, string json)
        {
            if (Listeners.ContainsKey(topic))
            {
                var providerList = Listeners[topic];

                foreach (var provider in providerList)
                {
                    provider.PropagateEvent(json);
                }
            }
        }
    }
}