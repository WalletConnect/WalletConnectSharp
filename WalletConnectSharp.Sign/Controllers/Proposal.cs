using System;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Controllers
{
    public class Proposal : Store<long,ProposalStruct>, IProposal
    {
        public Proposal(ICore core) : base(core, "proposal", WalletConnectSignClient.StoragePrefix)
        {
        }
    }
}