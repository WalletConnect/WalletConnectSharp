using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Sign.Models.Expirer;

namespace WalletConnectSharp.Sign
{
    public partial class Engine : IEnginePrivate, IEngine, IModule, IEvents
    {
        private const long ProposalExpiry = Clock.THIRTY_DAYS;
        private const long SessionExpiry = Clock.SEVEN_DAYS;
        private const int KeyLength = 32;
        
        public EventDelegator Events { get; }

        private bool _initialized = false;
        
        public ISignClient Client { get; }

        private IEnginePrivate PrivateThis => this;

        public string Name => $"{Client.Name}-engine";

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public Engine(ISignClient client)
        {
            this.Client = client;
            Events = new EventDelegator(this);
        }

        public async Task Init()
        {
            if (!this._initialized)
            {
                await PrivateThis.Cleanup();
                this.RegisterRelayerEvents();
                this.RegisterExpirerEvents();
                this._initialized = true;
            }
        }

        private async void RegisterExpirerEvents()
        {
            this.Client.Expirer.On<Expiration>(ExpirerEvents.Expired, ExpiredCallback);
        }

        private async void ExpiredCallback(object sender, GenericEvent<Expiration> e)
        {
            var target = new ExpirerTarget(e.EventData.Target);

            if (!string.IsNullOrWhiteSpace(target.Topic))
            {
                var topic = target.Topic;
                if (this.Client.Session.Keys.Contains(topic))
                {
                    await PrivateThis.DeleteSession(topic);
                    this.Client.Events.Trigger(EngineEvents.SessionExpire, topic);
                } 
                else if (this.Client.Pairing.Keys.Contains(topic))
                {
                    await PrivateThis.DeletePairing(topic);
                    this.Client.Events.Trigger(EngineEvents.PairingExpire, topic);
                }
            } 
            else if (target.Id != null)
            {
                await PrivateThis.DeleteProposal((long) target.Id);
            }
        }

        private void RegisterRelayerEvents()
        {
            // Register all Request Types
            HandleMessageType<PairingDelete, bool>(PrivateThis.OnPairingDeleteRequest, null);
            HandleMessageType<PairingPing, bool>(PrivateThis.OnPairingPingRequest, PrivateThis.OnPairingPingResponse);
            HandleMessageType<SessionPropose, SessionProposeResponse>(PrivateThis.OnSessionProposeRequest, PrivateThis.OnSessionProposeResponse);
            HandleMessageType<SessionSettle, bool>(PrivateThis.OnSessionSettleRequest, PrivateThis.OnSessionSettleResponse);
            HandleMessageType<SessionUpdate, bool>(PrivateThis.OnSessionUpdateRequest, PrivateThis.OnSessionUpdateResponse);
            HandleMessageType<SessionExtend, bool>(PrivateThis.OnSessionExtendRequest, PrivateThis.OnSessionExtendResponse);
            HandleMessageType<SessionDelete, bool>(PrivateThis.OnSessionDeleteRequest, null);
            HandleMessageType<SessionPing, bool>(PrivateThis.OnSessionPingRequest, PrivateThis.OnSessionPingResponse);

            this.Client.Core.Relayer.On<MessageEvent>(RelayerEvents.Message, RelayerMessageCallback);
        }

        private async void RelayerMessageCallback(object sender, GenericEvent<MessageEvent> e)
        {
            var topic = e.EventData.Topic;
            var message = e.EventData.Message;

            var payload = await this.Client.Core.Crypto.Decode<JsonRpcPayload>(topic, message);
            if (payload.IsRequest)
            {
                Events.Trigger($"request_{payload.Method}", e.EventData);
            }
            else if (payload.IsResponse)
            {
                Events.Trigger($"response_raw", new DecodedMessageEvent()
                {
                    Topic = topic,
                    Message = message,
                    Payload = payload
                });
            }
        }

        public TypedEventHandler<T, TR> SessionRequestEvents<T, TR>()
        {
            return SessionRequestEventHandler<T, TR>.GetInstance(this);
        }

        public void HandleSessionRequestMessageType<T, TR>(Func<string, JsonRpcRequest<SessionRequest<T>>, Task> requestCallback, Func<string, JsonRpcResponse<TR>, Task> responseCallback)
        {
            HandleMessageType(requestCallback, responseCallback);
        }
        
        public void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback, Func<string, JsonRpcResponse<bool>, Task> responseCallback)
        {
            HandleMessageType(requestCallback, responseCallback);
        }

        public async void HandleMessageType<T, TR>(Func<string, JsonRpcRequest<T>, Task> requestCallback, Func<string, JsonRpcResponse<TR>, Task> responseCallback)
        {
            var method = RpcMethodAttribute.MethodForType<T>();
            var rpcHistory = await this.Client.History.JsonRpcHistoryOfType<T, TR>();
            
            async void RequestCallback(object sender, GenericEvent<MessageEvent> e)
            {
                if (requestCallback == null) return;
                
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;

                var payload = await this.Client.Core.Crypto.Decode<JsonRpcRequest<T>>(topic, message);
                
                (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Set(topic, payload, null);

                await requestCallback(topic, payload);
            }
            
            async void ResponseCallback(object sender, GenericEvent<MessageEvent> e)
            {
                if (responseCallback == null) return;
                
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;

                var payload = await this.Client.Core.Crypto.Decode<JsonRpcResponse<TR>>(topic, message);

                await (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);

                await responseCallback(topic, payload);
            }

            async void InspectResponseRaw(object sender, GenericEvent<DecodedMessageEvent> e)
            {
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;

                var payload = e.EventData.Payload;

                try
                {
                    var record = await rpcHistory.Get(topic, payload.Id);

                    // ignored if we can't find anything in the history
                    if (record == null) return;
                    var resMethod = record.Request.Method;
                    
                    // Trigger the true response event, which will trigger ResponseCallback
                    Events.Trigger($"response_{resMethod}", new MessageEvent()
                    {
                        Topic = topic,
                        Message = message
                    });
                }
                catch(WalletConnectException err)
                {
                    if (err.CodeType != ErrorType.NO_MATCHING_KEY)
                        throw;
                    
                    // ignored if we can't find anything in the history
                }
            }

            Events.ListenFor<MessageEvent>($"request_{method}", RequestCallback);
            
            Events.ListenFor<MessageEvent>($"response_{method}", ResponseCallback);
            
            // Handle response_raw in this context
            // This will allow us to examine response_raw in every typed context registered
            Events.ListenFor<DecodedMessageEvent>($"response_raw", InspectResponseRaw);
        }

        public PublishOptions RpcRequestOptionsForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcRequestOptionsAttribute), true);
            if (attributes.Length != 1)
                throw new Exception($"Type {typeof(T).FullName} has no RpcRequestOptions attribute!");

            var opts = attributes.Cast<RpcRequestOptionsAttribute>().First();

            return new PublishOptions()
            {
                Prompt = opts.Prompt,
                Tag = opts.Tag,
                TTL = opts.TTL
            };
        }
        
        public PublishOptions RpcResponseOptionsForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcResponseOptionsAttribute), true);
            if (attributes.Length != 1)
                throw new Exception($"Type {typeof(T).FullName} has no RpcResponseOptions attribute!");

            var opts = attributes.Cast<RpcResponseOptionsAttribute>().First();

            return new PublishOptions()
            {
                Prompt = opts.Prompt,
                Tag = opts.Tag,
                TTL = opts.TTL
            };
        }

        async Task<long> IEnginePrivate.SendRequest<T, TR>(string topic, T parameters)
        {
            var method = RpcMethodAttribute.MethodForType<T>();

            var payload = new JsonRpcRequest<T>(method, parameters);

            var message = await this.Client.Core.Crypto.Encode(topic, payload);

            var opts = RpcRequestOptionsForType<T>();
            
            (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Set(topic, payload, null);

            // await is intentionally omitted here because of a possible race condition
            // where a response is received before the publish call is resolved
#pragma warning disable CS4014
            this.Client.Core.Relayer.Publish(topic, message, opts);
#pragma warning restore CS4014

            return payload.Id;
        }

        async Task IEnginePrivate.SendResult<T, TR>(long id, string topic, TR result)
        {
            var payload = new JsonRpcResponse<TR>(id, null, result);
            var message = await this.Client.Core.Crypto.Encode(topic, payload);
         
            var opts = RpcResponseOptionsForType<T>();
            await this.Client.Core.Relayer.Publish(topic, message, opts);
            await (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);
        }

        async Task IEnginePrivate.SendError<T, TR>(long id, string topic, ErrorResponse error)
        {
            var payload = new JsonRpcResponse<TR>(id, error, default);
            var message = await this.Client.Core.Crypto.Encode(topic, payload);
            var opts = RpcResponseOptionsForType<T>();
            await this.Client.Core.Relayer.Publish(topic, message, opts);
            await (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);
        }

        async Task IEnginePrivate.ActivatePairing(string topic)
        {
            var expiry = Clock.CalculateExpiry(ProposalExpiry);
            await this.Client.Pairing.Update(topic, new PairingStruct()
            {
                Active = true,
                Expiry = expiry
            });

            await PrivateThis.SetExpiry(topic, expiry);
        }

        async Task IEnginePrivate.DeleteSession(string topic)
        {
            var session = this.Client.Session.Get(topic);
            var self = session.Self;
            
            bool expirerHasDeleted = !this.Client.Expirer.Has(topic);
            bool sessionDeleted = !this.Client.Session.Keys.Contains(topic);
            bool hasKeypairDeleted = !(await this.Client.Core.Crypto.HasKeys(self.PublicKey));
            bool hasSymkeyDeleted = !(await this.Client.Core.Crypto.HasKeys(topic));

            await this.Client.Core.Relayer.Unsubscribe(topic);
            await Task.WhenAll(
                sessionDeleted ? Task.CompletedTask : this.Client.Session.Delete(topic, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED)),
                hasKeypairDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteKeyPair(self.PublicKey),
                hasSymkeyDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteSymKey(topic),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Expirer.Delete(topic)
            );
        }

        async Task IEnginePrivate.DeletePairing(string topic)
        {
            bool expirerHasDeleted = !this.Client.Expirer.Has(topic);
            bool pairingHasDeleted = !this.Client.Pairing.Keys.Contains(topic);
            bool symKeyHasDeleted = !(await this.Client.Core.Crypto.HasKeys(topic));
            
            await this.Client.Core.Relayer.Unsubscribe(topic);
            await Task.WhenAll(
                pairingHasDeleted ? Task.CompletedTask : this.Client.Pairing.Delete(topic, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED)),
                symKeyHasDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteSymKey(topic),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Expirer.Delete(topic)
            );
        }

        Task IEnginePrivate.DeleteProposal(long id)
        {
            bool expirerHasDeleted = !this.Client.Expirer.Has(id);
            bool proposalHasDeleted = !this.Client.Proposal.Keys.Contains(id);
            
            return Task.WhenAll(
                proposalHasDeleted ? Task.CompletedTask : this.Client.Proposal.Delete(id, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED)),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Expirer.Delete(id)
            );
        }

        async Task IEnginePrivate.SetExpiry(string topic, long expiry)
        {
            if (this.Client.Pairing.Keys.Contains(topic))
            {
                await this.Client.Pairing.Update(topic, new PairingStruct()
                {
                    Expiry = expiry
                });
            } 
            else if (this.Client.Session.Keys.Contains(topic))
            {
                await this.Client.Session.Update(topic, new SessionStruct()
                {
                    Expiry = expiry
                });
            }
            this.Client.Expirer.Set(topic, expiry);
        }

        async Task IEnginePrivate.SetProposal(long id, ProposalStruct proposal)
        {
            await this.Client.Proposal.Set(id, proposal);
            if (proposal.Expiry != null)
                this.Client.Expirer.Set(id, (long)proposal.Expiry);
        }

        Task IEnginePrivate.Cleanup()
        {
            List<string> sessionTopics = (from session in this.Client.Session.Values.Where(e => e.Expiry != null) where Clock.IsExpired(session.Expiry.Value) select session.Topic).ToList();
            List<string> pairingTopics = (from pair in this.Client.Pairing.Values.Where(e => e.Expiry != null) where Clock.IsExpired(pair.Expiry.Value) select pair.Topic).ToList();
            List<long> proposalIds = (from p in this.Client.Proposal.Values.Where(e => e.Expiry != null) where Clock.IsExpired(p.Expiry.Value) select p.Id.Value).ToList();

            return Task.WhenAll(
                sessionTopics.Select(t => PrivateThis.DeleteSession(t)).Concat(
                    pairingTopics.Select(t => PrivateThis.DeletePairing(t))
                ).Concat(
                    proposalIds.Select(id => PrivateThis.DeleteProposal(id))
                )
            );
        }

        async Task IEnginePrivate.OnSessionProposeRequest(string topic, JsonRpcRequest<SessionPropose> payload)
        {
            var @params = payload.Params;
            var id = payload.Id;
            try
            {
                var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
                var proposal = new ProposalStruct()
                {
                    Id = id,
                    PairingTopic = topic,
                    Expiry = expiry,
                    Proposer = @params.Proposer,
                    Relays = @params.Relays,
                    RequiredNamespaces = @params.RequiredNamespaces
                };
                await PrivateThis.SetProposal(id, proposal);
                this.Client.Events.Trigger(EngineEvents.SessionProposal, new JsonRpcRequest<ProposalStruct>()
                {
                    Id = id,
                    Params = proposal
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionPropose, SessionProposeResponse>(id, topic,
                    ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionProposeResponse(string topic, JsonRpcResponse<SessionProposeResponse> payload)
        {
            var id = payload.Id;
            if (payload.IsError)
            {
                await this.Client.Proposal.Delete(id, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED));
                this.Events.Trigger(EngineEvents.SessionConnect, payload);
            }
            else
            {
                var result = payload.Result;
                var proposal = this.Client.Proposal.Get(id);
                var selfPublicKey = proposal.Proposer.PublicKey;
                var peerPublicKey = result.ResponderPublicKey;

                var sessionTopic = await this.Client.Core.Crypto.GenerateSharedKey(
                    selfPublicKey,
                    peerPublicKey
                );
                var subscriptionId = await this.Client.Core.Relayer.Subscribe(sessionTopic);
                await PrivateThis.ActivatePairing(topic);
            }
        }

        async Task IEnginePrivate.OnSessionSettleRequest(string topic, JsonRpcRequest<SessionSettle> payload)
        {
            var id = payload.Id;
            var @params = payload.Params;
            try
            {
                await PrivateThis.IsValidSessionSettleRequest(@params);
                var relay = @params.Relay;
                var controller = @params.Controller;
                var expiry = @params.Expiry;
                var namespaces = @params.Namespaces;

                var session = new SessionStruct()
                {
                    Topic = topic,
                    Relay = relay,
                    Expiry = expiry,
                    Namespaces = namespaces,
                    Acknowledged = true,
                    Controller = controller.PublicKey,
                    Self = new Participant()
                    {
                        Metadata = this.Client.Metadata,
                        PublicKey = ""
                    },
                    Peer = new Participant()
                    {
                        PublicKey = controller.PublicKey,
                        Metadata = controller.Metadata
                    }
                };
                await PrivateThis.SendResult<SessionSettle, bool>(payload.Id, topic, true);
                this.Events.Trigger(EngineEvents.SessionConnect, session);
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionSettle, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionSettleResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            if (payload.IsError)
            {
                await this.Client.Session.Delete(topic, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED));
                this.Events.Trigger($"session_approve{id}", payload);
            }
            else
            {
                await this.Client.Session.Update(topic, new SessionStruct()
                {
                    Acknowledged = true
                });
                this.Events.Trigger($"session_approve{id}", payload); 
            }
        }

        async Task IEnginePrivate.OnSessionUpdateRequest(string topic, JsonRpcRequest<SessionUpdate> payload)
        {
            var @params = payload.Params;
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidUpdate(new UpdateParams()
                {
                    Namespaces = @params.Namespaces,
                    Topic = topic
                });

                await this.Client.Session.Update(topic, new SessionStruct()
                {
                    Namespaces = @params.Namespaces
                });

                await PrivateThis.SendResult<SessionUpdate, bool>(id, topic, true);
                this.Client.Events.Trigger(EngineEvents.SessionUpdate, new SessionUpdateEvent()
                {
                    Id = id,
                    Topic = topic,
                    Params = @params
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionUpdate, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionUpdateResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            this.Events.Trigger($"session_update{id}", payload);
        }

        async Task IEnginePrivate.OnSessionExtendRequest(string topic, JsonRpcRequest<SessionExtend> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidExtend(new ExtendParams()
                {
                    Topic = topic
                });
                await PrivateThis.SetExpiry(topic, Clock.CalculateExpiry(SessionExpiry));
                await PrivateThis.SendResult<SessionExtend, bool>(id, topic, true);
                this.Client.Events.Trigger(EngineEvents.SessionExtend, new SessionEvent()
                {
                    Id = id,
                    Topic = topic
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionExtend, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionExtendResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            this.Events.Trigger($"session_extend{id}", payload);
        }

        async Task IEnginePrivate.OnSessionPingRequest(string topic, JsonRpcRequest<SessionPing> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidPing(new PingParams()
                {
                    Topic = topic
                });
                await PrivateThis.SendResult<SessionPing, bool>(id, topic, true);
                this.Client.Events.Trigger(EngineEvents.SessionPing, new SessionEvent()
                {
                    Id = id,
                    Topic = topic
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionPing, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionPingResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            
            // put at the end of the stack to avoid a race condition
            // where session_ping listener is not yet initialized
            await Task.Delay(500);

            this.Events.Trigger($"session_ping{id}", payload);
        }

        async Task IEnginePrivate.OnPairingPingRequest(string topic, JsonRpcRequest<PairingPing> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidPing(new PingParams()
                {
                    Topic = topic
                });

                await PrivateThis.SendResult<PairingPing, bool>(id, topic, true);
                this.Client.Events.Trigger(EngineEvents.PairingPing, new SessionEvent()
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<PairingPing, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnPairingPingResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            
            // put at the end of the stack to avoid a race condition
            // where session_ping listener is not yet initialized
            await Task.Delay(500);

            this.Events.Trigger($"pairing_ping{id}", payload);
        }

        async Task IEnginePrivate.OnSessionDeleteRequest(string topic, JsonRpcRequest<SessionDelete> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidDisconnect(new DisconnectParams()
                {
                    Topic = topic,
                    Reason = payload.Params
                });

                await PrivateThis.SendResult<SessionDelete, bool>(id, topic, true);
                await PrivateThis.DeleteSession(topic);
                this.Client.Events.Trigger(EngineEvents.SessionDelete, new SessionEvent()
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionDelete, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnPairingDeleteRequest(string topic, JsonRpcRequest<PairingDelete> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidDisconnect(new DisconnectParams()
                {
                    Topic = topic,
                    Reason = payload.Params
                });

                await PrivateThis.SendResult<PairingDelete, bool>(id, topic, true);
                await PrivateThis.DeletePairing(topic);
                this.Client.Events.Trigger(EngineEvents.PairingDelete, new SessionEvent()
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<PairingDelete, bool>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionRequest<T, TR>(string topic, JsonRpcRequest<SessionRequest<T>> payload)
        {
            var id = payload.Id;
            var @params = payload.Params;
            try
            {
                await PrivateThis.IsValidRequest(new RequestParams<T>()
                {
                    Topic = topic,
                    ChainId = @params.ChainId,
                    Request = @params.Request
                });
                this.Client.Events.Trigger(EngineEvents.SessionRequest, new SessionRequestEvent<T>()
                {
                    Topic = topic,
                    Id = id,
                    ChainId = @params.ChainId,
                    Request = @params.Request
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionRequest<T>, TR>(id, topic, ErrorResponse.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionEventRequest<T>(string topic, JsonRpcRequest<SessionEvent<T>> payload)
        {
            var id = payload.Id;
            var @params = payload.Params;
            try
            {
                await PrivateThis.IsValidEmit(new EmitParams<T>()
                {
                    Topic = topic,
                    ChainId = @params.ChainId,
                    Event = @params.Event
                });
                this.Client.Events.Trigger(EngineEvents.SessionEvent, new EmitEvent<T>()
                {
                    Topic = topic,
                    Id = id,
                    Params = @params
                });
            }
            catch (WalletConnectException e)
            {
                await PrivateThis.SendError<SessionEvent<T>, object>(id, topic, ErrorResponse.FromException(e));
            }
        }

        private UriParameters ParseUri(string uri)
        {
            var pathStart = uri.IndexOf(":", StringComparison.Ordinal);
            int? pathEnd = uri.IndexOf("?", StringComparison.Ordinal) != -1 ? uri.IndexOf("?", StringComparison.Ordinal) : (int?)null;
            var protocol = uri.Substring(0, pathStart);

            string path;
            if (pathEnd != null) path = uri.Substring(pathStart + 1, (int)pathEnd - (pathStart + 1));
            else path = uri.Substring(pathStart + 1);

            var requiredValues = path.Split("@");
            string queryString = pathEnd != null ? uri.Substring((int)pathEnd) : "";
            var queryParams = Regex.Matches(queryString, "([^?=&]+)(=([^&]*))?").Cast<Match>()
                .ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);

            var result = new UriParameters()
            {
                Protocol = protocol,
                Topic = requiredValues[0],
                Version = int.Parse(requiredValues[1]),
                SymKey = queryParams["symKey"],
                Relay = new ProtocolOptions()
                {
                    Protocol = queryParams["relay-protocol"],
                    Data = queryParams.ContainsKey("relay-data") ? queryParams["relay-data"] : null,
                }
            };

            return result;
        }

        public async Task<ConnectedData> Connect(ConnectOptions options)
        {
            this.IsInitialized();
            await PrivateThis.IsValidConnect(options);
            var requiredNamespaces = options.RequiredNamespaces;
            var relays = options.Relays;
            var topic = options.PairingTopic;
            string uri = "";
            var active = false;

            if (!string.IsNullOrEmpty(topic))
            {
                var pairing = this.Client.Pairing.Get(topic);
                if (pairing.Active != null)
                    active = pairing.Active.Value;
            }

            if (string.IsNullOrEmpty(topic) || !active)
            {
                CreatePairingData CreatePairing = await this.CreatePairing();
                topic = CreatePairing.Topic;
                uri = CreatePairing.Uri;
            }

            var publicKey = await this.Client.Core.Crypto.GenerateKeyPair();
            var proposal = new SessionPropose()
            {
                RequiredNamespaces = requiredNamespaces,
                Relays = relays != null
                    ? new[] { relays }
                    : new[]
                    {
                        new ProtocolOptions()
                        {
                            Protocol = RelayProtocols.Default
                        }
                    },
                Proposer = new Participant()
                {
                    PublicKey = publicKey,
                    Metadata = this.Client.Metadata
                }
            };

            TaskCompletionSource<SessionStruct> approvalTask = new TaskCompletionSource<SessionStruct>();
            this.Events.ListenForOnce<SessionStruct>("session_connect", async (sender, e) =>
            {
                if (approvalTask.Task.IsCompleted)
                    return;
                
                var session = e.EventData;
                session.Self.PublicKey = publicKey;
                var completeSession = new SessionStruct()
                {
                    Acknowledged = session.Acknowledged,
                    Controller = session.Controller,
                    Expiry = session.Expiry,
                    RequiredNamespaces = requiredNamespaces,
                    Namespaces = session.Namespaces,
                    Peer = session.Peer,
                    Relay = session.Relay,
                    Self = session.Self,
                    Topic = session.Topic
                };
                await PrivateThis.SetExpiry(session.Topic, session.Expiry.Value);
                await Client.Session.Set(session.Topic, completeSession);
                
                if (!string.IsNullOrWhiteSpace(topic))
                {
                    await this.Client.Pairing.Update(topic, new PairingStruct()
                    {
                        PeerMetadata = session.Peer.Metadata
                    });
                }
                approvalTask.SetResult(completeSession);
            });
            
            this.Events.ListenForOnce<JsonRpcResponse<SessionProposeResponse>>("session_connect", (sender, e) =>
            {
                if (approvalTask.Task.IsCompleted)
                    return;
                if (e.EventData.IsError)
                {
                    approvalTask.SetException(e.EventData.Error.ToException());
                }
            });

            if (string.IsNullOrWhiteSpace(topic))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, $"connect() pairing topic: {topic}");
            }

            var id = await PrivateThis.SendRequest<SessionPropose, SessionProposeResponse>(topic, proposal);
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);

            await PrivateThis.SetProposal(id, new ProposalStruct()
            {
                Expiry = expiry,
                Id = id,
                Proposer = proposal.Proposer,
                Relays = proposal.Relays,
                RequiredNamespaces = proposal.RequiredNamespaces
            });

            return new ConnectedData()
            {
                Uri = uri,
                Approval = approvalTask.Task
            };
        }

        private async Task<CreatePairingData> CreatePairing()
        {
            byte[] symKeyRaw = new byte[KeyLength];
            RandomNumberGenerator.Fill(symKeyRaw);
            var symKey = symKeyRaw.ToHex();
            var topic = await this.Client.Core.Crypto.SetSymKey(symKey);
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
            var relay = new ProtocolOptions()
            {
                Protocol = RelayProtocols.Default
            };
            var pairing = new PairingStruct()
            {
                Topic = topic,
                Expiry = expiry,
                Relay = relay,
                Active = false,
            };
            var uri = $"{this.Client.Protocol}:{topic}@{this.Client.Version}?"
                .AddQueryParam("symKey", symKey)
                .AddQueryParam("relay-protocol", relay.Protocol);

            if (!string.IsNullOrWhiteSpace(relay.Data))
                uri = uri.AddQueryParam("relay-data", relay.Data);

            await this.Client.Pairing.Set(topic, pairing);
            await this.Client.Core.Relayer.Subscribe(topic);
            await PrivateThis.SetExpiry(topic, expiry);

            return new CreatePairingData()
            {
                Topic = topic,
                Uri = uri
            };
        }


        public async Task<ProposalStruct> Pair(PairParams pairParams)
        {
            IsInitialized();
            await PrivateThis.IsValidPair(pairParams);
            var uriParams = ParseUri(pairParams.Uri);

            var topic = uriParams.Topic;
            var symKey = uriParams.SymKey;
            var relay = uriParams.Relay;
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
            var pairing = new PairingStruct()
            {
                Topic = topic,
                Relay = relay,
                Expiry = expiry,
                Active = false,
            };

            TaskCompletionSource<ProposalStruct> sessionProposeTask = new TaskCompletionSource<ProposalStruct>();
            
            Client.Once(EngineEvents.SessionProposal,
                delegate(object sender, GenericEvent<JsonRpcRequest<ProposalStruct>> @event)
                {
                    var proposal = @event.EventData.Params;
                    if (topic == proposal.PairingTopic)
                        sessionProposeTask.SetResult(proposal);
                });

            await this.Client.Pairing.Set(topic, pairing);
            await this.Client.Core.Crypto.SetSymKey(symKey, topic);
            await this.Client.Core.Relayer.Subscribe(topic, new SubscribeOptions()
            {
                Relay = relay
            });
            await PrivateThis.SetExpiry(topic, expiry);

            return await sessionProposeTask.Task;
        }

        public async Task<IApprovedData> Approve(ApproveParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidApprove(@params);
            var id = @params.Id;
            var relayProtocol = @params.RelayProtocol;
            var namespaces = @params.Namespaces;
            var proposal = this.Client.Proposal.Get(id);
            var pairingTopic = proposal.PairingTopic;
            var proposer = proposal.Proposer;
            var requiredNamespaces = proposal.RequiredNamespaces;

            var selfPublicKey = await this.Client.Core.Crypto.GenerateKeyPair();
            var peerPublicKey = proposer.PublicKey;
            var sessionTopic = await this.Client.Core.Crypto.GenerateSharedKey(
                selfPublicKey,
                peerPublicKey
            );

            var sessionSettle = new SessionSettle()
            {
                Relay = new ProtocolOptions()
                {
                    Protocol = relayProtocol != null ? relayProtocol : "irn"
                },
                Namespaces = namespaces,
                Controller = new Participant()
                {
                    PublicKey = selfPublicKey,
                    Metadata = this.Client.Metadata
                },
                Expiry = Clock.CalculateExpiry(SessionExpiry)
            };

            await this.Client.Core.Relayer.Subscribe(sessionTopic);
            var requestId = await PrivateThis.SendRequest<SessionSettle, bool>(sessionTopic, sessionSettle);

            var acknowledgedTask = new TaskCompletionSource<SessionStruct>();
            
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_approve{requestId}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(this.Client.Session.Get(sessionTopic));
            });

            var session = new SessionStruct()
            {
                Topic = sessionTopic,
                Acknowledged = false,
                Self = sessionSettle.Controller,
                Peer = proposer,
                Controller = selfPublicKey,
                Expiry = sessionSettle.Expiry,
                Namespaces = sessionSettle.Namespaces,
                Relay = sessionSettle.Relay,
                RequiredNamespaces = requiredNamespaces
            };

            await this.Client.Session.Set(sessionTopic, session);
            await PrivateThis.SetExpiry(sessionTopic, Clock.CalculateExpiry(SessionExpiry));
            if (!string.IsNullOrWhiteSpace(pairingTopic))
                await this.Client.Pairing.Update(pairingTopic, new PairingStruct()
                {
                    PeerMetadata = session.Peer.Metadata
                });

            if (!string.IsNullOrWhiteSpace(pairingTopic) && id > 0)
            {
                await PrivateThis.SendResult<SessionPropose, SessionProposeResponse>(id, pairingTopic,
                    new SessionProposeResponse()
                    {
                        Relay = new ProtocolOptions()
                        {
                            Protocol = relayProtocol != null ? relayProtocol : "irn"
                        },
                        ResponderPublicKey = selfPublicKey
                    });
                await this.Client.Proposal.Delete(id, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED));
                await PrivateThis.ActivatePairing(pairingTopic);
            }
            
            return IApprovedData.FromTask(sessionTopic, acknowledgedTask.Task);
        }

        public async Task Reject(RejectParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidReject(@params);
            var id = @params.Id;
            var reason = @params.Reason;
            var proposal = this.Client.Proposal.Get(id);
            var pairingTopic = proposal.PairingTopic;

            if (!string.IsNullOrWhiteSpace(pairingTopic))
            {
                await PrivateThis.SendError<SessionPropose, SessionProposeResponse>(id, pairingTopic, reason);
                await this.Client.Proposal.Delete(id, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED));
            }
        }

        public async Task<IAcknowledgement> Update(UpdateParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidUpdate(@params);
            var topic = @params.Topic;
            var namespaces = @params.Namespaces;
            var id = await PrivateThis.SendRequest<SessionUpdate, bool>(topic, new SessionUpdate()
            {
                Namespaces = namespaces
            });

            TaskCompletionSource<bool> acknowledgedTask = new TaskCompletionSource<bool>();
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_update{id}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(e.EventData.Result);
            });

            await this.Client.Session.Update(topic, new SessionStruct()
            {
                Namespaces = namespaces
            });
            
            return IAcknowledgement.FromTask(acknowledgedTask.Task);
        }

        public async Task<IAcknowledgement> Extend(ExtendParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidExtend(@params);
            var topic = @params.Topic;
            var id = await PrivateThis.SendRequest<SessionExtend, bool>(topic, new SessionExtend());
            
            TaskCompletionSource<bool> acknowledgedTask = new TaskCompletionSource<bool>();
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_extend{id}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(e.EventData.Result);
            });

            await PrivateThis.SetExpiry(topic, Clock.CalculateExpiry(SessionExpiry));
            
            return IAcknowledgement.FromTask(acknowledgedTask.Task);
        }

        public async Task<TR> Request<T, TR>(string topic, T data, string chainId = null)
        {
            await IsValidSessionTopic(topic);

            var method = RpcMethodAttribute.MethodForType<T>();

            string defaultChainId;
            if (string.IsNullOrWhiteSpace(chainId))
            {
                var sessionData = Client.Session.Get(topic);
                var firstRequiredNamespace = sessionData.RequiredNamespaces.Keys.ToArray()[0];
                defaultChainId = sessionData.RequiredNamespaces[firstRequiredNamespace].Chains[0];
            }
            else
            {
                defaultChainId = chainId;
            }

            return await Request<T, TR>(new RequestParams<T>()
            {
                ChainId = defaultChainId,
                Request = new JsonRpcRequest<T>(method, data),
                Topic = topic
            });
        }

        public async Task<TR> Request<T, TR>(RequestParams<T> @params)
        {
            IsInitialized();
            await PrivateThis.IsValidRequest(@params);
            var chainId = @params.ChainId;
            var request = @params.Request;
            var topic = @params.Topic;

            var id = await PrivateThis.SendRequest<SessionRequest<T>, TR>(topic, new SessionRequest<T>()
            {
                ChainId = chainId,
                Request = request
            });
            
            var taskSource = new TaskCompletionSource<TR>();

            SessionRequestEvents<T, TR>()
                .FilterResponses((e) => e.Response.Id == id)
                .OnResponse += args =>
            {
                if (args.Response.IsError)
                    taskSource.SetException(args.Response.Error.ToException());
                else
                    taskSource.SetResult(args.Response.Result);

                return Task.CompletedTask;
            };

            return await taskSource.Task;
        }

        public async Task Respond<T, TR>(RespondParams<TR> @params)
        {
            IsInitialized();
            await PrivateThis.IsValidRespond(@params);
            var topic = @params.Topic;
            var response = @params.Response;
            var id = response.Id;
            if (response.IsError)
            {
                await PrivateThis.SendError<T, TR>(id, topic, response.Error);
            }
            else
            {
                await PrivateThis.SendResult<T, TR>(id, topic, response.Result);
            }
        }

        public async Task Emit<T>(EmitParams<T> @params)
        {
            IsInitialized();
            var topic = @params.Topic;
            var @event = @params.Event;
            var chainId = @params.ChainId;
            await PrivateThis.SendRequest<SessionEvent<T>, object>(topic, new SessionEvent<T>()
            {
                ChainId = chainId,
                Event = @event
            });
        }

        public async Task Ping(PingParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidPing(@params);

            var topic = @params.Topic;
            if (this.Client.Session.Keys.Contains(topic))
            {
                var id = await PrivateThis.SendRequest<SessionPing, bool>(topic, new SessionPing());
                var done = new TaskCompletionSource<bool>();
                this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_ping{id}", (sender, e) =>
                {
                    if (e.EventData.IsError)
                        done.SetException(e.EventData.Error.ToException());
                    else
                        done.SetResult(e.EventData.Result);
                });
                await done.Task;
            } 
            else if (this.Client.Pairing.Keys.Contains(topic))
            {
                var id = await PrivateThis.SendRequest<PairingPing, bool>(topic, new PairingPing());
                var done = new TaskCompletionSource<bool>();
                this.Events.ListenForOnce<JsonRpcResponse<bool>>($"pairing_ping{id}", (sender, e) =>
                {
                    if (e.EventData.IsError)
                        done.SetException(e.EventData.Error.ToException());
                    else
                        done.SetResult(e.EventData.Result);
                });
                await done.Task;
            }
        }

        public async Task Disconnect(DisconnectParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidDisconnect(@params);
            var topic = @params.Topic;
            
            var error = ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED);
            if (this.Client.Session.Keys.Contains(topic))
            {
                await PrivateThis.SendRequest<SessionDelete, bool>(topic, new SessionDelete()
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = error.Data
                });
                await PrivateThis.DeleteSession(topic);
            } 
            else if (this.Client.Pairing.Keys.Contains(topic))
            {
                await PrivateThis.SendRequest<PairingDelete, bool>(topic, new PairingDelete()
                {
                    Code = error.Code,
                    Data = error.Data,
                    Message = error.Message
                });
                await PrivateThis.DeletePairing(topic);
            }
        }

        public SessionStruct[] Find(FindParams @params)
        {
            IsInitialized();
            return this.Client.Session.Values.Where(s => IsSessionCompatible(s, @params)).ToArray();
        }
    }
}