using System.Text;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Logging;

namespace WalletConnectSharp.Storage
{
    /// <summary>
    /// A Storage strategy that stores both an in-memory dictionary as well serialized/deserializes
    /// all in-memory storage in a JSON file on the filesystem.
    /// </summary>
    public class FileSystemStorage : InMemoryStorage
    {
        /// <summary>
        /// The file path to store the JSON file
        /// </summary>
        public string FilePath { get; private set; }

        private readonly SemaphoreSlim _semaphoreSlim = new(1);

        /// <summary>
        /// A new FileSystemStorage module that reads/writes all storage
        /// values from storage
        /// </summary>
        /// <param name="filePath">The filepath to use, defaults to ~/.wc/store.json</param>
        public FileSystemStorage(string filePath = null)
        {
            if (filePath == null)
            {
                var home =
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                filePath = Path.Combine(home, ".wc", "store.json");
            }

            FilePath = filePath;
        }

        /// <summary>
        /// Initialize this storage module. Initialize the in-memory storage
        /// as well as loads in the JSON file
        /// </summary>
        /// <returns></returns>
        public override Task Init()
        {
            return Task.WhenAll(
                Load(), base.Init()
            );
        }

        /// <summary>
        /// The SetItem function stores the value based on the specified key and type. Will
        /// also update the JSON file
        /// </summary>
        /// <param name="key"> The key to store the value with</param>
        /// <param name="value">The value to store</param>
        /// <typeparam name="T">The type of data to store</typeparam>
        public override async Task SetItem<T>(string key, T value)
        {
            await base.SetItem<T>(key, value);
            await Save();
        }

        /// <summary>
        /// The RemoveItem function deletes the value stored based off of the specified key.
        /// Will also update the JSON file
        /// </summary>
        /// <param name="key">The key to delete the stored value pairing.</param>
        public override async Task RemoveItem(string key)
        {
            await base.RemoveItem(key);
            await Save();
        }

        /// <summary>
        /// Clear all entries in this storage. WARNING: This will delete all data!
        /// This will also update the JSON file
        /// </summary>
        public override async Task Clear()
        {
            await base.Clear();
            await Save();
        }

        private async Task Save()
        {
            var path = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string json;
            lock (entriesLock)
            {
                json = JsonConvert.SerializeObject(Entries,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            }

            try
            {
                if (!Disposed)
                    await _semaphoreSlim.WaitAsync();
                int count = 5;
                IOException lastException;
                do
                {
                    try
                    {
                        await File.WriteAllTextAsync(FilePath, json, Encoding.UTF8);
                        return;
                    }
                    catch (IOException e)
                    {
                        WCLogger.LogError($"Got error saving storage file: retries left {count}");
                        await Task.Delay(100);
                        count--;
                        lastException = e;
                    }
                } while (count > 0);

                throw lastException;
            }
            finally
            {
                if (!Disposed)
                    _semaphoreSlim.Release();
            }
        }

        private async Task Load()
        {
            if (!File.Exists(FilePath))
                return;

            string json;
            try
            {
                await _semaphoreSlim.WaitAsync();
                json = await File.ReadAllTextAsync(FilePath, Encoding.UTF8);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            
            // Hard fail here if the storage file is bad
            lock (entriesLock)
            {
                Entries = JsonConvert.DeserializeObject<Dictionary<string, object>>(json,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                _semaphoreSlim.Dispose();
            }

            Disposed = true;
        }
    }
}
