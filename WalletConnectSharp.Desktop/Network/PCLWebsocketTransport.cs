using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using IWebsocketClientLite.PCL;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using Websocket.Client;
using WebsocketClientLite.PCL;

namespace WalletConnectSharp.Desktop.Network
{
    public class PCLWebsocketTransport : ITransport
    {
        private MessageWebsocketRx client;
        //private var x;
        private EventDelegator _eventDelegator;

        private CancellationTokenSource checkConnectedCancellationToken = new CancellationTokenSource();

        public PCLWebsocketTransport(EventDelegator eventDelegator)
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
                return client != null && client.IsConnected;
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

            client = new MessageWebsocketRx();
            client.IgnoreServerCertificateErrors = true;
            var clientObservable = client.WebsocketConnectWithStatusObservable(new Uri(this.URL), hasClientPing: false);


            clientObservable.Do(x => {
                System.Diagnostics.Debug.Write("----STATUS----- " + x.state.ToString());
            }).Where(x => x.state == ConnectionStatus.DataframeReceived).Select(msg => Observable.FromAsync(() =>
            {
                OnMessageReceived(msg);
                return Task.CompletedTask;
            })).Subscribe();
            clientObservable.Where(x => x.state == ConnectionStatus.DataframeReceived).Subscribe(OnMessageReceived);

            await checkConnected(checkConnectedCancellationToken);
            int a = 1;
        }

        private async Task checkConnected(CancellationTokenSource _checkConnectedCancellationToken)
        {
            while (!_checkConnectedCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);

                if (Connected)
                {
                    System.Diagnostics.Debug.Write("--CONNECTED--");
                    _checkConnectedCancellationToken.Cancel();
                }
            }
        }

        private async void OnMessageReceived((IDataframe dataframe, ConnectionStatus state) responseMessage)
        {
            if (responseMessage.state == ConnectionStatus.DataframeReceived)
            {
                var json = responseMessage.dataframe.Message;

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
        }

        public async Task Close()
        {
            //await client.cl(WebSocketCloseStatus.NormalClosure, "");
        }

        public async Task SendMessage(NetworkMessage message)
        {
            var finalJson = JsonConvert.SerializeObject(message);

            await this.client.GetSender().SendText(finalJson);
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
