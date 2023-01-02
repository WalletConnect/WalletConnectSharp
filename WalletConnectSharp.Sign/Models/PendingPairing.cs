using System.Threading.Tasks;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models
{
    public class PendingPairing
    {
        public PairingStruct PairingData { get; internal set; }
        
        public ISignClient Client { get; }

        public Task<ProposalStruct> FetchProposal
        {
            get
            {
                return SessionProposeTask.Task;
            }
        }

        private TaskCompletionSource<ProposalStruct> SessionProposeTask = new TaskCompletionSource<ProposalStruct>();

        public PendingPairing(ISignClient client)
        {
            Client = client;
            
            Client.On<JsonRpcRequest<ProposalStruct>>(EngineEvents.SessionProposal, ProposalCallback);
        }

        private void ProposalCallback(object sender, GenericEvent<JsonRpcRequest<ProposalStruct>> e)
        {
            var proposal = e.EventData.Params;
            if (PairingData.Topic == proposal.PairingTopic)
                SessionProposeTask.SetResult(proposal);
        }
    }
}