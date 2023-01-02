using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Heartbeat;
using WalletConnectSharp.Events;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Expirer;

namespace WalletConnectSharp.Sign.Controllers
{
    public class Expirer : IExpirer
    {
        public static readonly string Version = "0.3";
        
        private Dictionary<string, Expiration> _expirations = new Dictionary<string, Expiration>();
        private bool initialized = false;
        private Expiration[] _cached = Array.Empty<Expiration>();
        private ICore _core;

        public string Name
        {
            get
            {
                return $"{_core.Name}-expirer";
            }
        }

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public EventDelegator Events { get; }

        public string StorageKey
        {
            get
            {
                return WalletConnectSignClient.StoragePrefix + Version + "//" + Name;
            }
        }

        public int Length
        {
            get
            {
                return _expirations.Count;
            }
        }

        public string[] Keys
        {
            get
            {
                return _expirations.Keys.ToArray();
            }
        }

        public Expiration[] Values
        {
            get
            {
                return _expirations.Values.ToArray();
            }
        }

        public Expirer(ICore core)
        {
            this._core = core;
            Events = new EventDelegator(this);
        }

        public async Task Init()
        {
            if (!initialized)
            {
                await Restore();

                foreach (var expiration in _cached)
                {
                    _expirations.Add(expiration.Target, expiration);
                }

                _cached = Array.Empty<Expiration>();
                RegisterEventListeners();
                initialized = true;
            }
        }

        public bool Has(string key)
        {
            try
            {
                var target = FormatTarget("topic", key);
                return GetExpiration(target) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Has(long key)
        {
            try
            {
                var target = FormatTarget("id", key);
                return GetExpiration(target) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Set(string key, long expiry)
        {
            IsInitialized();
            SetWithTarget("topic", key, expiry);
        }

        public void Set(long key, long expiry)
        {
            IsInitialized();
            SetWithTarget("id", key, expiry);
        }

        private void SetWithTarget(string targetType, object key, long expiry)
        {
            var target = FormatTarget(targetType, key);
            var expiration = new Expiration()
            {
                Target = target,
                Expiry = expiry
            };

            if (_expirations.ContainsKey(target))
                _expirations.Remove(target); // We cannot override, so remove first
            
            _expirations.Add(target, expiration);
            CheckExpiry(target, expiration);
            Events.Trigger(ExpirerEvents.Created, new ExpirerEventArgs()
            {
                Expiration = expiration,
                Target = target
            });
        }

        public Expiration Get(string key)
        {
            IsInitialized();
            return GetWithTarget("topic", key);
        }

        public Expiration Get(long key)
        {
            IsInitialized();
            return GetWithTarget("id", key);
        }

        private Expiration GetWithTarget(string targetType, object key)
        {
            var target = FormatTarget(targetType, key);
            return GetExpiration(target);
        }

        public Task Delete(string key)
        {
            IsInitialized();
            var target = FormatTarget("topic", key);
            var exists = this.Has(target);
            if (exists)
            {
                var expiration = this.GetExpiration(target);
                this._expirations.Remove(target);
                this.Events.Trigger(ExpirerEvents.Deleted, new ExpirerEventArgs()
                {
                    Expiration = expiration,
                    Target = target
                });
            }

            return Task.CompletedTask;
        }

        public Task Delete(long key)
        {
            IsInitialized();
            var target = FormatTarget("id", key);
            var exists = this.Has(target);
            if (exists)
            {
                var expiration = this.GetExpiration(target);
                this._expirations.Remove(target);
                this.Events.Trigger(ExpirerEvents.Deleted, new ExpirerEventArgs()
                {
                    Expiration = expiration,
                    Target = target
                });
            }

            return Task.CompletedTask;
        }

        private void DeleteWithTarget(string targetType, object key)
        {
            var target = FormatTarget(targetType, key);
            var exists = Has(target);
            if (exists)
            {
                var expiration = GetExpiration(target);
                _expirations.Remove(target);
                Events.Trigger(ExpirerEvents.Deleted, new ExpirerEventArgs()
                {
                    Target = target,
                    Expiration = expiration
                });
            }
        }

        private Task SetExpiration(Expiration[] expirations)
        {
            return _core.Storage.SetItem(StorageKey, expirations);
        }

        private async Task<Expiration[]> GetExpirations()
        {
            if (!(await _core.Storage.HasItem(StorageKey)))
                await _core.Storage.SetItem(StorageKey, Array.Empty<Expiration>());
            return await _core.Storage.GetItem<Expiration[]>(StorageKey);
        }

        private async void Persist()
        {
            await SetExpiration(Values);
            Events.Trigger(ExpirerEvents.Sync, this);
        }

        private async Task Restore()
        {
            var persisted = await GetExpirations();
            if (persisted == null) return;
            if (persisted.Length == 0) return;
            if (_expirations.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            _cached = persisted;
        }

        private Expiration GetExpiration(string target)
        {
            if (!_expirations.ContainsKey(target))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, $"{Name}: {target}");

            return _expirations[target];
        }

        private void CheckExpiry(string target, Expiration expiration)
        {
            var expiry = expiration.Expiry;
            var msToTimeout = (expiry * 1000) - DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (msToTimeout <= 0) Expire(target, expiration);
        }

        private void Expire(string target, Expiration expiration)
        {
            _expirations.Remove(target);
            Events.Trigger(ExpirerEvents.Expired, new ExpirerEventArgs()
            {
                Target = target,
                Expiration = expiration
            });
        }

        private void CheckExpirations()
        {
            foreach (var target in _expirations.Keys)
            {
                var expiration = _expirations[target];
                CheckExpiry(target, expiration);
            }
        }

        private void RegisterEventListeners()
        {
            _core.HeartBeat.On(HeartbeatEvents.Pulse, CheckExpirations);
            
            this.On(ExpirerEvents.Created, Persist);
            this.On(ExpirerEvents.Expired, Persist);
            this.On(ExpirerEvents.Deleted, Persist);
        }

        private void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }

        private string FormatTarget(string targetType, object key)
        {
            if (key is string s && s.StartsWith($"{targetType}:")) return s;

            switch (targetType.ToLower())
            {
                case "topic":
                    if (!(key is string))
                        throw new ArgumentException("Value must be \"string\" for expirer target type: topic");
                    break;
                case "id":
                    if (!key.IsNumericType())
                        throw new ArgumentException("Value must be \"number\" for expirer target type: id");
                    break;
                default:
                    throw new ArgumentException($"Unknown expirer target type: ${targetType}");
            }
            
            return $"{targetType}:{key}";
        }
    }
}