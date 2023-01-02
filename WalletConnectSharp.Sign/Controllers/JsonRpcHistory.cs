using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.History;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Controllers
{
    public class JsonRpcHistory<T, TR> : IJsonRpcHistory<T, TR>
    {

        public static readonly string Version = "0.3";
        
        public EventDelegator Events { get; }
        
        public string Name
        {
            get
            {
                return $"{_core.Name}-history-of-type-{typeof(T).Name}";
            }
        }

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public string StorageKey
        {
            get
            {
                return WalletConnectSignClient.StoragePrefix + Version + "//" + Name;
            }
        }

        private JsonRpcRecord<T, TR>[] _cached = Array.Empty<JsonRpcRecord<T, TR>>();
        private Dictionary<long, JsonRpcRecord<T, TR>> _records = new Dictionary<long, JsonRpcRecord<T, TR>>();
        private bool initialized = false;
        private ICore _core;

        public IReadOnlyDictionary<long, JsonRpcRecord<T, TR>> Records
        {
            get
            {
                return _records;
            }
        }

        public int Size
        {
            get
            {
                return _records.Count;
            }
        }

        public long[] Keys
        {
            get
            {
                return _records.Keys.ToArray();
            }
        }

        public JsonRpcRecord<T, TR>[] Values
        {
            get
            {
                return _records.Values.ToArray();
            }
        }

        public RequestEvent<T>[] Pending
        {
            get
            {
                var pending = Values.Where(jrr => jrr.Response == null);

                return pending.Select(RequestEvent<T>.FromPending).ToArray();
            }
        }

        public JsonRpcHistory(ICore core)
        {
            _core = core;
            Events = new EventDelegator(this);
        }

        public async Task Init()
        {
            if (!initialized)
            {
                await Restore();
                foreach (var record in _cached)
                {
                    _records.Add(record.Id, record);
                }

                _cached = Array.Empty<JsonRpcRecord<T, TR>>();
                RegisterEventListeners();
                initialized = true;
            }
        }

        public void Set(string topic, IJsonRpcRequest<T> request, string chainId)
        {
            IsInitialized();
            if (_records.ContainsKey(request.Id)) return;

            var record = new JsonRpcRecord<T, TR>(request)
            {
                Id = request.Id,
                Topic = topic,
                ChainId = chainId,
            };
            _records.Add(record.Id, record);
            Events.Trigger(HistoryEvents.Created, record);
        }

        public Task<JsonRpcRecord<T, TR>> Get(string topic, long id)
        {
            IsInitialized();

            var record = GetRecord(id);
            if (topic != record.Topic)
            {
                throw WalletConnectException.FromType(ErrorType.MISMATCHED_TOPIC, $"{Name}: {id}");
            }
            
            return Task.FromResult<JsonRpcRecord<T, TR>>(record);
        }

        public Task Resolve(IJsonRpcResult<TR> response)
        {
            IsInitialized();
            if (!_records.ContainsKey(response.Id)) return Task.CompletedTask;

            var record = GetRecord(response.Id);
            if (record.Response != null) return Task.CompletedTask;

            record.Response = response;
            Events.Trigger(HistoryEvents.Updated, record);
            return Task.CompletedTask;
        }

        public void Delete(string topic, long? id)
        {
            IsInitialized();

            foreach (var record in Values)
            {
                if (record.Topic == topic)
                {
                    if (id != null && record.Id != id) continue;
                    _records.Remove(record.Id);
                    Events.Trigger(HistoryEvents.Deleted, record);
                }
            }
        }

        public Task<bool> Exists(string topic, long id)
        {
            IsInitialized();
            if (_records.ContainsKey(id)) return Task.FromResult<bool>(false);
            var record = GetRecord(id);

            return Task.FromResult(record.Topic == topic);
        }

        private Task SetJsonRpcRecords(JsonRpcRecord<T, TR>[] records)
        {
            return _core.Storage.SetItem(StorageKey, records);
        }

        private async Task<JsonRpcRecord<T, TR>[]> GetJsonRpcRecords()
        {
            if (await _core.Storage.HasItem(StorageKey))
                return await _core.Storage.GetItem<JsonRpcRecord<T, TR>[]>(StorageKey);

            return Array.Empty<JsonRpcRecord<T, TR>>();
        }

        private JsonRpcRecord<T, TR> GetRecord(long id)
        {
            IsInitialized();

            if (!_records.ContainsKey(id))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, new {Tag = $"{Name}: {id}"});
            }

            return _records[id];
        }

        private async Task Persist()
        {
            await SetJsonRpcRecords(Values);
            Events.Trigger(HistoryEvents.Sync, new object());
        }

        private async Task Restore()
        {
            var persisted = await GetJsonRpcRecords();
            if (persisted == null)
                return;
            if (persisted.Length == 0)
                return;
            if (_records.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            _cached = persisted;
        }

        private void RegisterEventListeners()
        {
            this.On<JsonRpcRecord<T, TR>>(HistoryEvents.Created, SaveRecordCallback);

            this.On<JsonRpcRecord<T, TR>>(HistoryEvents.Updated, SaveRecordCallback);

            this.On<JsonRpcRecord<T, TR>>(HistoryEvents.Deleted, SaveRecordCallback);
        }

        private async void SaveRecordCallback(object sender, GenericEvent<JsonRpcRecord<T, TR>> @event)
        {
            await Persist();
        }

        private void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }
    }
}