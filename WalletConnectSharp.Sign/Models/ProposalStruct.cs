using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign.Models
{
    public struct ProposalStruct : IKeyHolder<long>
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonIgnore]
        public long Key
        {
            get
            {
                return (long) Id;
            }
        }
        
        [JsonProperty("expiry")]
        public long? Expiry { get; set; }
        
        [JsonProperty("relays")]
        public ProtocolOptions[] Relays { get; set; }
        
        [JsonProperty("proposer")]
        public Participant Proposer { get; set; }
        
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
        
        [JsonProperty("pairingTopic")]
        public string PairingTopic { get; set; }

        public ApproveParams ApproveProposal(string approvedAccount, ProtocolOptions protocolOption = null)
        {
            return ApproveProposal(new[] { approvedAccount }, protocolOption);
        }

        public ApproveParams ApproveProposal(string[] approvedAccounts, ProtocolOptions protocolOption = null)
        {
            if (Id == null)
                throw new Exception("Proposal has no set Id");
            if (protocolOption == null)
                protocolOption = Relays[0];
            else if (Relays.All(r => r.Protocol != protocolOption.Protocol))
                throw new Exception("Requested protocol not in proposal");

            var relayProtocol = protocolOption.Protocol;

            var namespaces = new Namespaces();
            foreach (var key in RequiredNamespaces.Keys)
            {
                var rn = RequiredNamespaces[key];
                var allAccounts = (from chain in rn.Chains from account in approvedAccounts select $"{chain}:{account}").ToArray();
                
                namespaces.Add(key, new Namespace()
                {
                    Accounts = allAccounts,
                    Events = rn.Events,
                    Methods = rn.Methods
                });
            }

            return new ApproveParams()
            {
                Id = Id.Value,
                RelayProtocol = relayProtocol,
                Namespaces = namespaces
            };
        }
    }
}