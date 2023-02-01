using System.Security.Cryptography;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign
{
    public partial class Engine
    {
        async Task<CreatePairingData> CreatePairing()
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
        
        async Task<long> IEnginePrivate.SendRequest<T, TR>(string topic, T parameters)
        {
            var method = RpcMethodAttribute.MethodForType<T>();

            var payload = new JsonRpcRequest<T>(method, parameters);

            var message = await this.Client.Core.Crypto.Encode(topic, payload);

            var opts = RpcRequestOptionsFromType<T, TR>();
            
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
            var opts = RpcResponseOptionsFromTypes<T, TR>();
            await this.Client.Core.Relayer.Publish(topic, message, opts);
            await (await this.Client.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);
        }

        async Task IEnginePrivate.SendError<T, TR>(long id, string topic, ErrorResponse error)
        {
            var payload = new JsonRpcResponse<TR>(id, error, default);
            var message = await this.Client.Core.Crypto.Encode(topic, payload);
            var opts = RpcResponseOptionsFromTypes<T, TR>();
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
    }
}
