using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Sign.Models.Expirer;

namespace WalletConnectSharp.Sign
{
    public partial class Engine
    {
        async void ExpiredCallback(object sender, GenericEvent<Expiration> e)
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
        
        async void RelayerMessageCallback(object sender, GenericEvent<MessageEvent> e)
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
                await PrivateThis.IsValidUpdate(topic, @params.Namespaces);

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
                await PrivateThis.IsValidExtend(topic);
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
                await PrivateThis.IsValidPing(topic);
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
                await PrivateThis.IsValidPing(topic);

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
                await PrivateThis.IsValidDisconnect(topic, payload.Params);

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
                await PrivateThis.IsValidDisconnect(topic, payload.Params);

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
                await PrivateThis.IsValidRequest(topic, @params.Request, @params.ChainId);
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
                await PrivateThis.IsValidEmit(topic, @params.Event, @params.ChainId);
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
    }
}
