using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Controllers
{
    public class Store<TKey, TValue> : IStore<TKey, TValue> where TValue : IKeyHolder<TKey>
    {
        private bool initialized;
        private Dictionary<TKey, TValue> map = new Dictionary<TKey, TValue>();
        private TValue[] cached = Array.Empty<TValue>();
        public ICore Core { get; }
        
        public string StoragePrefix { get; }
        
        public Func<TValue, TKey> GetKey { get; }

        public string Version
        {
            get
            {
                return "0.3";
            }
        }
        public string Name { get; }
        public string Context { get; }

        public string StorageKey
        {
            get
            {
                return StoragePrefix + Version + "//" + Name;
            }
        }

        public int Length
        {
            get
            {
                return map.Count;
            }
        }

        public TKey[] Keys
        {
            get
            {
                return map.Keys.ToArray();
            }
        }

        public TValue[] Values
        {
            get
            {
                return map.Values.ToArray();
            }
        }

        public Store(ICore core, string name, string storagePrefix = null, Func<TValue, TKey> getKey = null)
        {
            Core = core;

            name = $"{core.Name}-{name}";
            Name = name;
            Context = name;
            
            if (storagePrefix == null)
                StoragePrefix = WalletConnectSharp.Core.Core.STORAGE_PREFIX;
            else
                StoragePrefix = storagePrefix;

            GetKey = getKey;
        }

        public async Task Init()
        {
            if (!initialized)
            {
                await Restore();

                foreach (var value in cached)
                {
                    if (value != null)
                        map.Add(value.Key, value);
                }

                cached = Array.Empty<TValue>();
                initialized = true;
            }
        }

        public Task Set(TKey key, TValue value)
        {
            IsInitialized();

            if (map.ContainsKey(key))
            {
                return Update(key, value);
            }
            map.Add(key, value);
            return Persist();
        }

        public TValue Get(TKey key)
        {
            IsInitialized();
            var value = GetData(key);
            return value;
        }

        public Task Update(TKey key, TValue update)
        {
            IsInitialized();
            
            // Partial updates aren't built into C#
            // However, we can use reflection to sort of
            // get the same thing
            try
            {
                // First, we check if we even have a value to reference
                var previousValue = Get(key);

                // Find all properties the type TKey has
                Type t = typeof(TValue);
                var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

                // Loop through all of them
                foreach (var prop in properties)
                {
                    // Grab the updated value
                    var @value = prop.GetValue(update, null);
                    // If it exists (its not null), then set it
                    if (@value != null)
                    {
                        prop.SetValue(previousValue, null);
                    }
                }
                
                // Now, set the update variable to be the new modified 
                // previousValue object
                update = previousValue;
            }
            catch (WalletConnectException e)
            {
                // ignored if no previous value exists
            }

            map.Remove(key);
            map.Add(key, update);

            return Persist();
        }

        public Task Delete(TKey key, ErrorResponse reason)
        {
            IsInitialized();

            if (!map.ContainsKey(key)) return Task.CompletedTask;

            map.Remove(key);

            return Persist();
        }

        protected virtual Task SetDataStore(TValue[] data)
        {
            return Core.Storage.SetItem<TValue[]>(StorageKey, data);
        }

        protected virtual async Task<TValue[]> GetDataStore()
        {
            if (await Core.Storage.HasItem(StorageKey))
                return await Core.Storage.GetItem<TValue[]>(StorageKey);

            return Array.Empty<TValue>();
        }

        protected virtual TValue GetData(TKey key)
        {
            if (!map.ContainsKey(key))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, $"{Name}: {key}");
            }

            return map[key];
        }

        protected virtual Task Persist()
        {
            return SetDataStore(Values);
        }

        protected virtual async Task Restore()
        {
            var persisted = await GetDataStore();
            if (persisted == null) return;
            if (persisted.Length == 0) return;
            if (map.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            cached = persisted;
        }

        protected virtual void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }
    }
}