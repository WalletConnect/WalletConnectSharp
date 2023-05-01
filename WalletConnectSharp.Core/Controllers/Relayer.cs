using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Subscriber;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Network.Websocket;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// The Relayer module handles the interaction with the WalletConnect relayer server.
    /// Each Relayer module uses a Publisher, Subscriber and a JsonRPCProvider.
    /// </summary>
    public class Relayer : IRelayer
    {
        /// <summary>
        /// The default relay server URL used when no relay URL is given
        /// </summary>
        public static readonly string DEFAULT_RELAY_URL = "wss://relay.walletconnect.com";
        
        /// <summary>
        /// The EventDelegaor this Relayer module is using
        /// </summary>
        public EventDelegator Events { get; }

        /// <summary>
        /// The Name of this Relayer module
        /// </summary>
        public string Name
        {
            get
            {
                return $"{Core.Name}-relayer";
            }
        }

        /// <summary>
        /// The context string this Relayer module is using
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// The ICore module that is using this Relayer module
        /// </summary>
        public ICore Core { get; }
        
        /// <summary>
        /// The ISubscriber module that this Relayer module is using
        /// </summary>
        public ISubscriber Subscriber { get; }
        
        /// <summary>
        /// The IPublisher module that this Relayer module is using
        /// </summary>
        public IPublisher Publisher { get; }
        
        /// <summary>
        /// The IMessageTracker module that this Relayer module is using
        /// </summary>
        public IMessageTracker Messages { get; }
        
        /// <summary>
        /// The IJsonRpcProvider module that this Relayer module is using
        /// </summary>
        public IJsonRpcProvider Provider { get; private set; }
        
        /// <summary>
        /// Whether this Relayer is connected
        /// </summary>
        public bool Connected { get; }
        
        /// <summary>
        /// Whether this Relayer is currently connecting
        /// </summary>
        public bool Connecting { get; }
        
        private string relayUrl;
        private string projectId;
        private bool initialized;
        
        /// <summary>
        /// Create a new Relayer with the given RelayerOptions.
        /// </summary>
        /// <param name="opts">The options that must be specified. This includes the ICore module
        /// using this module, the RelayURL (optional) and the project Id</param>
        public Relayer(RelayerOptions opts)
        {
            Core = opts.Core;
            Events = new EventDelegator(this);
            Messages = new MessageTracker(Core);
            Subscriber = new Subscriber(this);
            Publisher = new Publisher(this);

            relayUrl = opts.RelayUrl;
            if (string.IsNullOrWhiteSpace(relayUrl))
            {
                relayUrl = DEFAULT_RELAY_URL;
            }

            projectId = opts.ProjectId;
        }
        
        /// <summary>
        /// Initialize this Relayer module. This will initialize all sub-modules
        /// and connect the backing IJsonRpcProvider.
        /// </summary>
        public async Task Init()
        {
            var auth = await this.Core.Crypto.SignJwt(this.relayUrl);
            Provider = CreateProvider(auth);

            await Task.WhenAll(
                Messages.Init(), Provider.Connect(), Subscriber.Init()
            );

            RegisterEventListeners();
            
            initialized = true;
        }

        protected virtual IJsonRpcProvider CreateProvider(string auth)
        {
            return new JsonRpcProvider(
                new WebsocketConnection(
                    RelayUrl.FormatRelayRpcUrl(
                        relayUrl,
                        IRelayer.Protocol,
                        IRelayer.Version.ToString(),
                        SDKConstants.SDK_VERSION,
                        projectId,
                        auth
                    )
                )
            );
        }

        protected virtual void RegisterEventListeners()
        {
            Provider.On<string>(ProviderEvents.RawRequestMessage, (sender, @event) =>
            {
                OnProviderPayload(@event.EventData);
            });
            
            Provider.On(ProviderEvents.Connect, () =>
            {
                Events.Trigger(RelayerEvents.Connect, new object());
            });
            
            Provider.On(ProviderEvents.Disconnect, async () =>
            {
                Events.Trigger(RelayerEvents.Disconnect, new object());
                
                
                // Attempt to reconnect after one second
                await Task.Delay(1000);
                await Provider.Connect();
            });

            Provider.On<object>(ProviderEvents.Error, (sender, @event) =>
            {
                Events.Trigger(RelayerEvents.Error, @event.EventData);
            });
        }

        protected virtual async void OnProviderPayload(string payloadJson)
        {
            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(payloadJson);
            
            if (payload != null && payload.IsRequest && payload.Method.EndsWith("_subscription"))
            {
                var @event = JsonConvert.DeserializeObject<JsonRpcRequest<JsonRpcSubscriptionParams>>(payloadJson);

                var messageEvent = new MessageEvent()
                {
                    Message = @event.Params.Data.Message,
                    Topic = @event.Params.Data.Topic
                };

                await AcknowledgePayload(payload);
                await OnMessageEvent(messageEvent);
            }
        }

        protected virtual async Task<bool> ShouldIgnoreMessageEvent(MessageEvent messageEvent)
        {
            if (!(await Subscriber.IsSubscribed(messageEvent.Topic))) return true;

            var exists = Messages.Has(messageEvent.Topic, messageEvent.Message);
            return exists;
        }

        protected virtual Task RecordMessageEvent(MessageEvent messageEvent)
        {
            return Messages.Set(messageEvent.Topic, messageEvent.Message);
        }

        protected virtual async Task OnMessageEvent(MessageEvent messageEvent)
        {
            if (await ShouldIgnoreMessageEvent(messageEvent)) return;

            Events.Trigger(RelayerEvents.Message, messageEvent);
            await RecordMessageEvent(messageEvent);
        }

        protected virtual async Task AcknowledgePayload(JsonRpcPayload payload)
        {
            var response = new JsonRpcResponse<bool>()
            {
                Id = payload.Id,
                Result = true
            };
            await Provider.Connection.SendResult(response, this);
        }

        /// <summary>
        /// Publish a message to this Relayer in the given topic (optionally) specifying
        /// PublishOptions.
        /// </summary>
        /// <param name="topic">The topic to publish the message in</param>
        /// <param name="message">The message to publish</param>
        /// <param name="opts">(Optional) Publish options to specify TTL and tag</param>
        public async Task Publish(string topic, string message, PublishOptions opts = null)
        {
            IsInitialized();
            await Publisher.Publish(topic, message, opts);
            await RecordMessageEvent(new MessageEvent()
            {
                Topic = topic,
                Message = message
            });
        }

        /// <summary>
        /// Subscribe to a given topic optionally specifying Subscribe options
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="opts">(Optional) Subscribe options that specify protocol options</param>
        /// <returns></returns>
        public Task<string> Subscribe(string topic, SubscribeOptions opts = null)
        {
            IsInitialized();
            return Subscriber.Subscribe(topic, opts);
        }

        /// <summary>
        /// Unsubscribe to a given topic optionally specify unsubscribe options
        /// </summary>
        /// <param name="topic">Tbe topic to unsubscribe to</param>
        /// <param name="opts">(Optional) Unsubscribe options specifying protocol options</param>
        /// <returns></returns>
        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null)
        {
            IsInitialized();
            return Subscriber.Unsubscribe(topic, opts);
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
