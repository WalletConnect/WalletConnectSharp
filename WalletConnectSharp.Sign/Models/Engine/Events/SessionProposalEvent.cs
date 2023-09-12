using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Verify;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    public class SessionProposalEvent
    {
        [JsonProperty("id")]
        public long Id;
        
        [JsonProperty("params")]
        public ProposalStruct Proposal;
        
        [JsonProperty("verifyContext")]
        public VerifiedContext VerifiedContext;
    }
}
