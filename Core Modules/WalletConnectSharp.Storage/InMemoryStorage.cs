using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Storage
{
    public class InMemoryStorage : IKeyValueStorage
    {
        protected readonly object entriesLock = new object();
        protected Dictionary<string, object> Entries = new Dictionary<string, object>();
        private bool _initialized = false;
        protected bool Disposed;

        public virtual Task Init()
        {
            _initialized = true;
            return Task.CompletedTask;
        }

        public virtual Task<string[]> GetKeys()
        {
            IsInitialized();
            lock (entriesLock)
            {
                return Task.FromResult(Entries.Keys.ToArray());
            }
        }

        public virtual async Task<T[]> GetEntriesOfType<T>()
        {
            IsInitialized();
            // GetEntries is thread-safe
            return (await GetEntries()).OfType<T>().ToArray();
        }

        public virtual Task<object[]> GetEntries()
        {
            IsInitialized();
            lock (entriesLock)
            {
                return Task.FromResult(Entries.Values.ToArray());
            }
        }

        public virtual Task<T> GetItem<T>(string key)
        {
            IsInitialized();
            lock (entriesLock)
            {
                return Task.FromResult(Entries[key] is T ? (T)Entries[key] : default);
            }
        }

        public virtual Task SetItem<T>(string key, T value)
        {
            IsInitialized();
            lock (entriesLock)
            {
                Entries[key] = value;
            }

            return Task.CompletedTask;
        }

        public virtual Task RemoveItem(string key)
        {
            IsInitialized();
            lock (entriesLock)
            {
                Entries.Remove(key);
            }

            return Task.CompletedTask;
        }

        public virtual Task<bool> HasItem(string key)
        {
            IsInitialized();
            lock (entriesLock)
            {
                return Task.FromResult(Entries.ContainsKey(key));
            }
        }

        public virtual Task Clear()
        {
            IsInitialized();
            lock (entriesLock)
            {
                Entries.Clear();
            }

            return Task.CompletedTask;
        }

        protected void IsInitialized()
        {
            if (!_initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, "Storage");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }
    }
}
