using System.Threading.Tasks;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign.Interfaces
{
    public interface IEngineTasks
    {
        Task<ConnectedData> Connect(ConnectOptions options);

        Task<ProposalStruct> Pair(PairParams pairParams);

        Task<IApprovedData> Approve(ApproveParams @params);

        Task Reject(RejectParams @params);

        Task<IAcknowledgement> Update(UpdateParams @params);

        Task<IAcknowledgement> Extend(ExtendParams @params);

        Task<TR> Request<T, TR>(string topic, T data, string chainId = null);
        
        Task<TR> Request<T, TR>(RequestParams<T> @params);

        Task Respond<T, TR>(RespondParams<TR> @params);

        Task Emit<T>(EmitParams<T> @params);

        Task Ping(PingParams @params);

        Task Disconnect(DisconnectParams @params);

        SessionStruct[] Find(FindParams @params);
    }
}