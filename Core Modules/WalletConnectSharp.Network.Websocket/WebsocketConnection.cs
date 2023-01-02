using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using Websocket.Client;

namespace WalletConnectSharp.Network.Websocket
{
    /// <summary>
    /// A JSON RPC connection using Websocket.Client library + EventDelegator
    /// </summary>
    public class WebsocketConnection : IJsonRpcConnection, IModule
    {
        private EventDelegator _delegator;
        private WebsocketClient _socket;
        private string _url;
        private bool _registering;
        public Guid _context;

        public TimeSpan OpenTimeout = TimeSpan.FromSeconds(60);

        public string Url
        {
            get
            {
                return _url;
            }
        }

        public string Name
        {
            get
            {
                return "websocket-connection";
            }
        }

        public string Context
        {
            get
            {
                return _context.ToString();
            }
        }

        public EventDelegator Events
        {
            get
            {
                return _delegator;
            }
        }
        
        public bool Connected
        {
            get
            {
                return _socket != null;
            }
        }

        public bool Connecting
        {
            get
            {
                return _registering;
            }
        }

        public WebsocketConnection(string url)
        {
            if (!Validation.IsWsUrl(url))
                throw new ArgumentException("Provided URL is not compatible with WebSocket connection: " + url);
            
            _context = Guid.NewGuid();
            this._url = url;
            _delegator = new EventDelegator(this);
        }

        public async Task Open()
        {
            await Register(_url);
        }

        public async Task Open<T>(T options)
        {
            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                await Register(options as string);
            }

            await Open();
        }

        private async Task<WebsocketClient> Register(string url)
        {
            if (!Validation.IsWsUrl(url))
            {
                throw new ArgumentException("Provided URL is not compatible with WebSocket connection: " + url);
            }

            if (_registering)
            {
                TaskCompletionSource<WebsocketClient> registeringTask =
                    new TaskCompletionSource<WebsocketClient>(TaskCreationOptions.None);
                
                Events.ListenForOnce("register_error",
                    delegate(object sender, GenericEvent<Exception> @event)
                    {
                        registeringTask.SetException(@event.EventData);
                    });
                
                Events.ListenForOnce("open",
                    delegate(object sender, GenericEvent<WebsocketClient> @event)
                    {
                        registeringTask.SetResult(@event.EventData);
                    });

                await registeringTask.Task;

                return registeringTask.Task.Result;
            }

            this._url = url;
            this._registering = true;

            try
            {
                _socket = new WebsocketClient(new Uri(_url));

                await _socket.Start().WithTimeout(OpenTimeout, "Unavailable WS RPC url at " + _url);
                OnOpen(_socket);
                return _socket;
            }
            catch (Exception e)
            {
                Events.Trigger(WebsocketConnectionEvents.RegisterError, e);
                OnClose(new DisconnectionInfo(DisconnectionType.Error, WebSocketCloseStatus.Empty, e.Message, null, e));

                throw;
            }
        }

        private void OnOpen(WebsocketClient socket)
        {
            socket.MessageReceived.Subscribe(OnPayload);
            socket.DisconnectionHappened.Subscribe(OnDisconnect);

            this._socket = socket;
            this._registering = false;
            Events.Trigger(WebsocketConnectionEvents.Open, _socket);
        }

        private void OnDisconnect(DisconnectionInfo obj)
        {
            if (obj.Exception != null)
                Events.Trigger(WebsocketConnectionEvents.Error, obj.Exception);
            
            OnClose(obj);
        }
        
        private void OnClose(DisconnectionInfo obj)
        {
            if (this._socket == null)
                return;
            
            //_socket.Dispose();
            this._socket = null;
            this._registering = false;
            Events.Trigger(WebsocketConnectionEvents.Close, obj);
        }

        private void OnPayload(ResponseMessage obj)
        {
            string json = null;
            switch (obj.MessageType)
            {
                case WebSocketMessageType.Binary:
                    return;
                case WebSocketMessageType.Text:
                    json = obj.Text;
                    break;
                case WebSocketMessageType.Close:
                    return;
            }

            if (string.IsNullOrWhiteSpace(json)) return;

            Events.Trigger(WebsocketConnectionEvents.Payload, json);
        }

        public async Task Close()
        {
            if (_socket == null)
                throw new IOException("Connection already closed");

            await _socket.Stop(WebSocketCloseStatus.NormalClosure, "Close Invoked");
            
            OnClose(new DisconnectionInfo(DisconnectionType.Exit, WebSocketCloseStatus.Empty, "Close Invoked", null, null));
        }

        public async Task SendRequest<T>(IJsonRpcRequest<T> requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }
        }

        public async Task SendResult<T>(IJsonRpcResult<T> requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }
        }

        public async Task SendError(IJsonRpcError requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<object>(requestPayload, e);
            }
        }

        public async void Dispose()
        {
            if (Connected)
            {
                await Close();
            }
        }

        private string addressNotFoundError = "getaddrinfo ENOTFOUND";
        private string connectionRefusedError = "connect ECONNREFUSED";
        private void OnError<T>(IJsonRpcPayload ogPayload, Exception e)
        {
            var exception = e.Message.Contains(addressNotFoundError) || e.Message.Contains(connectionRefusedError)
                ? new IOException("Unavailable WS RPC url at " + _url) : e;

            var message = exception.Message;
            var payload = new JsonRpcResponse<T>(ogPayload.Id, new ErrorResponse()
            {
                Code = exception.HResult,
                Data = null,
                Message = message
            }, default(T));

            //Trigger the payload event, converting the new JsonRpcResponse object to JSON string
            Events.Trigger(WebsocketConnectionEvents.Payload, JsonConvert.SerializeObject(payload));
        }
    }
}