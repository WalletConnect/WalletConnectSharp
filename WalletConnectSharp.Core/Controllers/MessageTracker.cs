using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Controllers
{
    public class MessageTracker : IMessageTracker
    {
        public static readonly string Version = "0.3";
        
        public string Name
        {
            get
            {
                return $"{_core.Name}-messages";
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
                return Core.STORAGE_PREFIX + Version + "//" + Name;
            }
        }

        private bool initialized;
        private ICore _core;
        public Dictionary<string, MessageRecord> Messages { get; private set; }

        public MessageTracker(ICore core)
        {
            this._core = core;
        }
        
        public async Task Init()
        {
            if (!initialized)
            {
                var messages = await GetRelayerMessages();

                if (messages != null)
                {
                    Messages = messages;
                }

                initialized = true;
            }
        }

        public async Task<string> Set(string topic, string message)
        {
            IsInitialized();

            var hash = HashUtils.HashMessage(message);
            
            MessageRecord messages;
            if (Messages.ContainsKey(topic))
                messages = Messages[topic];
            else
            {
                messages = new MessageRecord();
                Messages.Add(topic, messages);
            }

            if (messages.ContainsKey(hash))
                return hash;
            
            messages.Add(hash, message);
            await Persist();
            return hash;
        }

        public Task<MessageRecord> Get(string topic)
        {
            IsInitialized();

            return Task.FromResult(Messages.ContainsKey(topic) ? Messages[topic] : new MessageRecord());
        }

        public bool Has(string topic, string message)
        {
            IsInitialized();

            if (!Messages.ContainsKey(topic)) return false;
            
            var hash = HashUtils.HashMessage(message);

            return Messages[topic].ContainsKey(hash);

        }

        public async Task Delete(string topic)
        {
            IsInitialized();

            Messages.Remove(topic);

            await Persist();
        }

        private async Task SetRelayerMessages(Dictionary<string, MessageRecord> messages)
        {
            await _core.Storage.SetItem(StorageKey, messages);
        }

        private async Task<Dictionary<string, MessageRecord>> GetRelayerMessages()
        {
            if (await _core.Storage.HasItem(StorageKey))
                return await _core.Storage.GetItem<Dictionary<string, MessageRecord>>(StorageKey);

            return new Dictionary<string, MessageRecord>();
        }

        private Task Persist()
        {
            return SetRelayerMessages(Messages);
        }

        private void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, this.Name);
            }
        }
    }
}