using System.Threading.Tasks;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    internal interface IEnginePrivate
    {
        internal Task<long> SendRequest<T, TR>(string topic, T parameters) where T : IWcMethod;

        internal Task SendResult<T, TR>(long id, string topic, TR result);

        internal Task SendError<T, TR>(long id, string topic, ErrorResponse error);

        internal Task ActivatePairing(string topic);

        internal Task DeleteSession(string topic);

        internal Task DeletePairing(string topic);

        internal Task DeleteProposal(long id);

        internal Task SetExpiry(string topic, long expiry);

        internal Task SetProposal(long id, ProposalStruct proposal);

        internal Task Cleanup();

        internal Task OnSessionProposeRequest(string topic, JsonRpcRequest<SessionPropose> payload);

        internal Task OnSessionProposeResponse(string topic, JsonRpcResponse<SessionProposeResponse> payload);

        internal Task OnSessionSettleRequest(string topic, JsonRpcRequest<SessionSettle> payload);

        internal Task OnSessionSettleResponse(string topic, JsonRpcResponse<bool> payload);

        internal Task OnSessionUpdateRequest(string topic, JsonRpcRequest<SessionUpdate> payload);

        internal Task OnSessionUpdateResponse(string topic, JsonRpcResponse<bool> payload);

        internal Task OnSessionExtendRequest(string topic, JsonRpcRequest<SessionExtend> payload);

        internal Task OnSessionExtendResponse(string topic, JsonRpcResponse<bool> payload);

        internal Task OnSessionPingRequest(string topic, JsonRpcRequest<SessionPing> payload);

        internal Task OnSessionPingResponse(string topic, JsonRpcResponse<bool> payload);

        internal Task OnPairingPingRequest(string topic, JsonRpcRequest<PairingPing> payload);

        internal Task OnPairingPingResponse(string topic, JsonRpcResponse<bool> payload);

        internal Task OnSessionDeleteRequest(string topic, JsonRpcRequest<SessionDelete> payload);

        internal Task OnPairingDeleteRequest(string topic, JsonRpcRequest<PairingDelete> payload);

        internal Task OnSessionRequest<T, TR>(string topic, JsonRpcRequest<SessionRequest<T>> payload);

        internal Task OnSessionEventRequest<T>(string topic, JsonRpcRequest<SessionEvent<T>> payload);

        internal Task IsValidConnect(ConnectOptions options);

        internal Task IsValidPair(PairParams pairParams);

        internal Task IsValidSessionSettleRequest(SessionSettle settle);

        internal Task IsValidApprove(ApproveParams @params);

        internal Task IsValidReject(RejectParams @params);

        internal Task IsValidUpdate(UpdateParams @params);

        internal Task IsValidExtend(ExtendParams @params);

        internal Task IsValidRequest<T>(RequestParams<T> @params);

        internal Task IsValidRespond<T>(RespondParams<T> @params);

        internal Task IsValidPing(PingParams @params);

        internal Task IsValidEmit<T>(EmitParams<T> @params);

        internal Task IsValidDisconnect(DisconnectParams @params);
    }
}