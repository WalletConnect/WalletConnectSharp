using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Heartbeat;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Subscriber;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Controllers
{
    public class Subscriber : ISubscriber
    {
        public static class SubscriberEvents
        {
            public static readonly string Created = "subscription_created";
            public static readonly string Deleted = "subscription_deleted";
            public static readonly string Expired = "subscription_expired";
            public static readonly string Disabled = "subscription_disabled";
            public static readonly string Sync = "subscription_sync";
        }
        
        public EventDelegator Events { get; }

        public string Name
        {
            get
            {
                return $"{_relayer.Name}-subscription";
            }
        }

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public string Version
        {
            get
            {
                return "0.3";
            }
        }

        private Dictionary<string, SubscriberActive> _subscriptions = new Dictionary<string, SubscriberActive>();
        private Dictionary<string, SubscriberParams> pending = new Dictionary<string, SubscriberParams>();
        public IReadOnlyDictionary<string, SubscriberActive> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
        }

        private TopicMap _topicMap = new TopicMap();

        public ISubscriberMap TopicMap
        {
            get
            {
                return _topicMap;
            }
        }

        public int Length
        {
            get
            {
                return _subscriptions.Count;
            }
        }

        public string[] Ids
        {
            get
            {
                return _subscriptions.Keys.ToArray();
            }
        }

        public SubscriberActive[] Values
        {
            get
            {
                return _subscriptions.Values.ToArray();
            }
        }

        public string[] Topics
        {
            get
            {
                return _topicMap.Topics;
            }
        }

        public string StorageKey
        {
            get
            {
                return Core.STORAGE_PREFIX + Version + "//" + Name;
            }
        }

        private IRelayer _relayer;
        private bool initialized;
        private SubscriberActive[] cached = Array.Empty<SubscriberActive>();

        public Subscriber(IRelayer relayer)
        {
            _relayer = relayer;
            
            Events = new EventDelegator(this);
        }
        
        public async Task Init()
        {
            if (!initialized)
            {
                await Restore();
                await Reset();
                RegisterEventListeners();
                OnEnabled();
            }
        }

        protected virtual void RegisterEventListeners()
        {
            _relayer.Core.HeartBeat.On<object>(HeartbeatEvents.Pulse, (sender, @event) =>
            {
                CheckPending();
            });
            
            _relayer.Provider.On<object>(ProviderEvents.Connect, (sender, @event) =>
            {
                OnConnect();
            });
            
            _relayer.Provider.On<object>(ProviderEvents.Disconnect, (sender, @event) =>
            {
                OnDisconnect();
            });
            
            this.On<object>(SubscriberEvents.Created, AsyncPersist);

            this.On<object>(SubscriberEvents.Deleted, AsyncPersist);
        }

        protected virtual async void AsyncPersist(object sender, GenericEvent<object> @event)
        {
            await Persist();
        }
        
        
        protected virtual async Task Persist()
        {
            await SetRelayerSubscriptions(Values);
            Events.Trigger(SubscriberEvents.Sync, new object());
        }

        protected virtual async Task<SubscriberActive[]> GetRelayerSubscriptions()
        {
            if (await _relayer.Core.Storage.HasItem(StorageKey))
                return await _relayer.Core.Storage.GetItem<SubscriberActive[]>(StorageKey);

            return Array.Empty<SubscriberActive>();
        }

        protected virtual async Task SetRelayerSubscriptions(SubscriberActive[] subscriptions)
        {
            await _relayer.Core.Storage.SetItem(StorageKey, subscriptions);
        }

        protected virtual async Task Restore()
        {
            var persisted = await GetRelayerSubscriptions();

            if (persisted.Length == 0) return;

            if (Subscriptions.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            cached = persisted;
        }

        protected virtual async void CheckPending()
        {
            foreach (var @params in pending.Values)
            {
                var id = await RpcSubscribe<string>(@params.Topic, @params.Relay);
                OnSubscribe(id, @params);
            }
        }

        protected virtual async Task Reset()
        {
            if (cached.Length == 0)
                return;

            await Task.WhenAll(
                cached.Select(Resubscribe)
            );
        }

        protected virtual async Task Resubscribe(SubscriberActive subscription)
        {
            if (!Ids.Contains(subscription.Id))
            {
                var @params = new SubscriberParams()
                {
                    Relay = subscription.Relay,
                    Topic = subscription.Topic
                };

                if (pending.ContainsKey(@params.Topic))
                    pending.Remove(@params.Topic);
                
                pending.Add(@params.Topic, @params);

                var id = await RpcSubscribe<string>(@params.Topic, @params.Relay);
                OnResubscribe(id, @params);
            }
        }

        protected virtual Task<string> RpcSubscribe<TR>(string topic, ProtocolOptions relay)
        {
            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<JsonRpcSubscriberParams>()
            {
                Method = api.Subscribe,
                Params = new JsonRpcSubscriberParams()
                {
                    Topic = topic
                }
            };
            
            return _relayer.Provider.Request<JsonRpcSubscriberParams, string>(request);
        }

        protected virtual Task RpcUnsubscribe(string topic, string id, ProtocolOptions relay)
        {
            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<JsonRpcUnsubscribeParams>()
            {
                Method = api.Unsubscribe,
                Params = new JsonRpcUnsubscribeParams()
                {
                    Id = id,
                    Topic = topic
                }
            };

            return _relayer.Provider.Request<JsonRpcUnsubscribeParams, object>(request);
        }

        protected virtual void OnEnabled()
        {
            cached = Array.Empty<SubscriberActive>();
            initialized = true;
        }

        protected virtual void OnDisconnect()
        {
            OnDisable();
        }

        protected virtual void OnDisable()
        {
            cached = Values;
            _subscriptions.Clear();
            _topicMap.Clear();
            initialized = false;
        }

        protected virtual async void OnConnect()
        {
            await Reset();
            OnEnabled();
        }

        protected virtual void OnSubscribe(string id, SubscriberParams @params)
        {
            SetSubscription(id, new SubscriberActive()
            {
                Id = id,
                Relay = @params.Relay,
                Topic = @params.Topic
            });

            pending.Remove(@params.Topic);
        }

        protected virtual void OnResubscribe(string id, SubscriberParams @params)
        {
            AddSubscription(id, new SubscriberActive()
            {
                Id = id,
                Relay = @params.Relay,
                Topic = @params.Topic
            });

            pending.Remove(@params.Topic);
        }

        protected virtual async Task OnUnsubscribe(string topic, string id, ErrorResponse reason)
        {
            // TODO Figure out how to do this
            //Events.RemoveListener(id);

            if (HasSubscription(id, topic))
            {
                DeleteSubscription(id, reason);
            }
            
            await _relayer.Messages.Delete(topic);
        }

        protected virtual void SetSubscription(string id, SubscriberActive subscription)
        {
            if (_subscriptions.ContainsKey(id)) return;
            
            AddSubscription(id, subscription);
        }

        protected virtual void AddSubscription(string id, SubscriberActive subscription)
        {
            if (_subscriptions.ContainsKey(id))
                _subscriptions.Remove(id);
            
            _subscriptions.Add(id, subscription);
            _topicMap.Set(subscription.Topic, id);
            Events.Trigger(SubscriberEvents.Created, subscription);
        }

        protected virtual Task UnsubscribeByTopic(string topic, UnsubscribeOptions opts = null)
        {
            if (opts == null)
            {
                opts = new UnsubscribeOptions()
                {
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }

            return Task.WhenAll(
                Ids.Select(id => UnsubscribeById(topic, id, opts))
            );
        }

        protected virtual void DeleteSubscription(string id, ErrorResponse reason)
        {
            var subscription = GetSubscription(id);
            _subscriptions.Remove(id);
            _topicMap.Delete(id);
            Events.Trigger(SubscriberEvents.Deleted, new SubscriberDeleted()
            {
                Id = id,
                Reason = reason,
                Relay = subscription.Relay,
                Topic = subscription.Topic
            });
        }

        protected virtual async Task UnsubscribeById(string topic, string id, UnsubscribeOptions opts)
        {
            if (opts == null)
            {
                opts = new UnsubscribeOptions()
                {
                    Id = id,
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }

            await RpcUnsubscribe(topic, id, opts.Relay);
            ErrorResponse reason = null;
            await OnUnsubscribe(topic, id, reason);
        }

        protected virtual SubscriberActive GetSubscription(string id)
        {
            if (!_subscriptions.ContainsKey(id))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, Name + ": " + id);

            return _subscriptions[id];
        }

        protected virtual bool HasSubscription(string id, string topic)
        {
            var result = false;
            try
            {
                var subscriptions = GetSubscription(id);
                result = subscriptions.Topic == topic;
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        protected virtual void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }

        public async Task<string> Subscribe(string topic, SubscribeOptions opts = null)
        {
            if (opts == null)
            {
                opts = new SubscribeOptions()
                {
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }
            
            IsInitialized();

            var @params = new SubscriberParams()
            {
                Relay = opts.Relay,
                Topic = topic
            };
            
            pending.Add(topic, @params);
            var id = await RpcSubscribe<string>(topic, @params.Relay);
            OnSubscribe(id, @params);
            return id;
        }

        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null)
        {
            IsInitialized();

            if (opts != null && !string.IsNullOrWhiteSpace(opts.Id))
            {
                return UnsubscribeById(topic, opts.Id, opts);
            }

            return UnsubscribeByTopic(topic, opts);
        }

        public Task<bool> IsSubscribed(string topic)
        {
            if (Topics.Contains(topic)) return Task.FromResult(true);

            return Task.Run(async delegate()
            {
                var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                while (true)
                {
                    if (!pending.ContainsKey(topic) && Topics.Contains(topic))
                    {
                        return true;
                    }

                    var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if (currentTime - startTime >= 5)
                    {
                        return false;
                    }

                    await Task.Delay(20);
                }
            });
        }
    }
}