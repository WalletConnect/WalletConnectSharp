using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// A full implementation of the IJsonRpcProvider interface using the EventDelegator
    /// </summary>
    public class JsonRpcProvider : IJsonRpcProvider, IModule
    {
        private IJsonRpcConnection _connection;
        private EventDelegator _delegator;
        private bool _hasRegisteredEventListeners;
        private Guid _context;
        private bool _connectingStarted;
        private TaskCompletionSource<bool> Connecting = new TaskCompletionSource<bool>();
        private long _lastId;

        public bool IsConnecting
        {
            get
            {
                return _connectingStarted && !Connecting.Task.IsCompleted;
            }
        }
        
        public IJsonRpcConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        public string Name
        {
            get
            {
                return "json-rpc-provider";
            }
        }

        public string Context
        {
            get
            {
                //TODO Get context from logger
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

        public JsonRpcProvider(IJsonRpcConnection connection)
        {
            _context = Guid.NewGuid();
            this._delegator = new EventDelegator(this);
            this._connection = connection;
            if (this._connection.Connected)
            {
                RegisterEventListeners();
            }
        }

        public async Task Connect(string connection)
        {
            if (this._connection.Connected)
            {
                await this._connection.Close();
            }
            
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
            _connectingStarted = true;

            await this._connection.Open(connection);
            FinalizeConnection(this._connection);
        }

        public async Task Connect(IJsonRpcConnection connection)
        {
            if (this._connection == connection && connection.Connected) return;
            if (this._connection.Connected)
            {
                await this._connection.Close();
            }
            
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
            _connectingStarted = true;

            await connection.Open();

            FinalizeConnection(connection);
        }
        
        private void FinalizeConnection(IJsonRpcConnection connection)
        {
            this._connection = connection;
            RegisterEventListeners();
            Events.Trigger(ProviderEvents.Connect, connection);
            Connecting.SetResult(true);
            _connectingStarted = false;
        }

        public async Task Connect<T>(T @params)
        {
            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                await Connect(@params as string);
                return;
            }

            if (typeof(IJsonRpcConnection).IsAssignableFrom(typeof(T)))
            {
                await Connect(@params as IJsonRpcConnection);
                return;
            }

            await _connection.Open(@params);
            
            FinalizeConnection(_connection);
        }

        public async Task Connect()
        {
            await Connect(_connection);
        }

        public async Task Disconnect()
        {
            await _connection.Close();
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
        }

        public async Task<TR> Request<T, TR>(IRequestArguments<T> requestArgs, object context = null)
        {
            if (IsConnecting)
                await Connecting.Task;
            else if (!_connectingStarted && !_connection.Connected)
            {
                await Connect(_connection);
            }

            long? id = null;
            if (requestArgs is IJsonRpcRequest<T>)
            {
                id = ((IJsonRpcRequest<T>)requestArgs).Id;
                if (id == 0)
                    id = null; // An id of 0 is null
            }
            var request = new JsonRpcRequest<T>(requestArgs.Method, requestArgs.Params, id);

            TaskCompletionSource<TR> requestTask = new TaskCompletionSource<TR>(TaskCreationOptions.None);
            
            Events.ListenForAndDeserialize<JsonRpcResponse<TR>>(request.Id.ToString(),
                delegate(object sender, GenericEvent<JsonRpcResponse<TR>> @event)
                {
                    if (requestTask.Task.IsCompleted)
                        return;
                    
                    var result = @event.EventData;

                    if (result.Error != null)
                    {
                        requestTask.SetException(new IOException(result.Error.Message));
                    }
                    else
                    {
                        requestTask.SetResult(result.Result);
                    }
                });
            
            Events.ListenFor(request.Id.ToString(), delegate(object sender, GenericEvent<WalletConnectException> @event)
            {
                if (requestTask.Task.IsCompleted)
                    return;
                
                var exception = @event.EventData;
                if (exception != null)
                {
                    requestTask.SetException(exception);
                }
            });

            _lastId = request.Id;
            
            await _connection.SendRequest(request, context);

            await requestTask.Task;

            return requestTask.Task.Result;
        }

        protected void RegisterEventListeners()
        {
            if (_hasRegisteredEventListeners) return;
            
            _connection.On<string>("payload", OnPayload);
            _connection.On<object>("close", OnConnectionDisconnected);
            _connection.On<Exception>("error", OnConnectionError);
            _hasRegisteredEventListeners = true;
        }

        private void OnConnectionError(object sender, GenericEvent<Exception> e)
        {
            Events.Trigger(ProviderEvents.Error, e.EventData);
        }

        private void OnConnectionDisconnected(object sender, GenericEvent<object> e)
        {
            Events.TriggerType(ProviderEvents.Disconnect, e.EventData, e.EventData.GetType());
        }

        private void OnPayload(object sender, GenericEvent<string> e)
        {
            var json = e.EventData;

            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(json);

            if (payload == null)
            {
                throw new IOException("Invalid payload: " + json);
            }

            if (payload.Id == 0)
                payload.Id = _lastId;
            
            Events.Trigger(ProviderEvents.Payload, payload);

            if (payload.IsRequest)
            {
                Events.Trigger(ProviderEvents.RawRequestMessage, json);
            }
            else
            {
                if (payload.IsError)
                {
                    var errorPayload = JsonConvert.DeserializeObject<JsonRpcError>(json);
                    Events.Trigger(payload.Id.ToString(), errorPayload.Error.ToException());
                }
                else
                {
                    Events.Trigger(payload.Id.ToString(), json);
                }
            }
        }
    }
}