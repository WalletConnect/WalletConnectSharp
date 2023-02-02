using System.Security.Cryptography;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign
{
    public partial class Engine
    {
        async Task IEnginePrivate.DeleteSession(string topic)
        {
            var session = this.Client.Session.Get(topic);
            var self = session.Self;
            
            bool expirerHasDeleted = !this.Client.Core.Expirer.Has(topic);
            bool sessionDeleted = !this.Client.Session.Keys.Contains(topic);
            bool hasKeypairDeleted = !(await this.Client.Core.Crypto.HasKeys(self.PublicKey));
            bool hasSymkeyDeleted = !(await this.Client.Core.Crypto.HasKeys(topic));

            await this.Client.Core.Relayer.Unsubscribe(topic);
            await Task.WhenAll(
                sessionDeleted ? Task.CompletedTask : this.Client.Session.Delete(topic, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED)),
                hasKeypairDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteKeyPair(self.PublicKey),
                hasSymkeyDeleted ? Task.CompletedTask : this.Client.Core.Crypto.DeleteSymKey(topic),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Core.Expirer.Delete(topic)
            );
        }

        Task IEnginePrivate.DeleteProposal(long id)
        {
            bool expirerHasDeleted = !this.Client.Core.Expirer.Has(id);
            bool proposalHasDeleted = !this.Client.Proposal.Keys.Contains(id);
            
            return Task.WhenAll(
                proposalHasDeleted ? Task.CompletedTask : this.Client.Proposal.Delete(id, ErrorResponse.FromErrorType(ErrorType.USER_DISCONNECTED)),
                expirerHasDeleted ? Task.CompletedTask : this.Client.Core.Expirer.Delete(id)
            );
        }

        async Task IEnginePrivate.SetExpiry(string topic, long expiry)
        {
            if (this.Client.Session.Keys.Contains(topic))
            {
                await this.Client.Session.Update(topic, new SessionStruct()
                {
                    Expiry = expiry
                });
            }
            this.Client.Core.Expirer.Set(topic, expiry);
        }

        async Task IEnginePrivate.SetProposal(long id, ProposalStruct proposal)
        {
            await this.Client.Proposal.Set(id, proposal);
            if (proposal.Expiry != null)
                this.Client.Core.Expirer.Set(id, (long)proposal.Expiry);
        }

        Task IEnginePrivate.Cleanup()
        {
            List<string> sessionTopics = (from session in this.Client.Session.Values.Where(e => e.Expiry != null) where Clock.IsExpired(session.Expiry.Value) select session.Topic).ToList();
            List<long> proposalIds = (from p in this.Client.Proposal.Values.Where(e => e.Expiry != null) where Clock.IsExpired(p.Expiry.Value) select p.Id.Value).ToList();

            return Task.WhenAll(
                sessionTopics.Select(t => PrivateThis.DeleteSession(t)).Concat(
                    proposalIds.Select(id => PrivateThis.DeleteProposal(id))
                )
            );
        }
    }
}
