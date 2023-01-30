using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents functions the Sign client Engine can perform. These
    /// functions exist in both the Engine and in the Sign client. 
    /// </summary>
    public interface IEngineTasks
    {
        /// <summary>
        /// Connect a dApp with the given ConnectOptions. At a minimum, you must specified a RequiredNamespace. 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<ConnectedData> Connect(ConnectOptions options);

        Task<ProposalStruct> Pair(string uri);
        
        Task<ProposalStruct> Pair(PairParams pairParams);

        Task<IApprovedData> Approve(ProposalStruct proposalStruct, params string[] approvedAddresses);

        Task<IApprovedData> Approve(ApproveParams @params);

        Task Reject(RejectParams @params);

        Task Reject(ProposalStruct @params, string message = null);

        Task Reject(ProposalStruct @params, ErrorResponse error);

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
