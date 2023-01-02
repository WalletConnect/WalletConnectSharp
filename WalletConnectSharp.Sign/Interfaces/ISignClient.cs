using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces
{
    public interface ISignClient : IModule, IEvents, IEngineTasks
    {
        Metadata Metadata { get; }
        
        ICore Core { get; }
        
        IEngine Engine { get; }
        
        IPairing Pairing { get; }
        
        ISession Session { get; }
        
        IProposal Proposal { get; }
        
        IJsonRpcHistoryFactory History { get; }
        
        IExpirer Expirer { get; }
        
        SignClientOptions Options { get; }
        
        string Protocol { get; }
        
        int Version { get; }
    }
}