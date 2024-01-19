using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents functions the Sign client Engine can perform. These
    /// functions exist in both the Engine and in the Sign client. 
    /// </summary>
    public interface IEngineAPI
    {
        /// <summary>
        /// This event is invoked when the given session has expired
        /// Event Side: dApp & Wallet
        /// </summary>
        event EventHandler<SessionStruct> SessionExpired;

        /// <summary>
        /// This event is invoked when the given pairing has expired
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<PairingEvent> PairingExpired;

        /// <summary>
        /// This event is invoked when a new session is proposed. This is usually invoked
        /// after a new pairing has been activated from a URI
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<SessionProposalEvent> SessionProposed;

        /// <summary>
        /// This event is invoked when a proposed session has been connected to a wallet. This event is
        /// triggered after the session has been approved by a wallet
        /// Event Side: dApp
        /// </summary>
        event EventHandler<SessionStruct> SessionConnected;

        /// <summary>
        /// This event is invoked when a proposed session connection failed with an error
        /// Event Side: dApp
        /// </summary>
        event EventHandler<Exception> SessionConnectionErrored;

        /// <summary>
        /// This event is invoked when a given session sent a update request. 
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<SessionUpdateEvent> SessionUpdateRequest;

        /// <summary>
        /// This event is invoked when a given session sent a extend request.
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<SessionEvent> SessionExtendRequest;
        
        /// <summary>
        /// This event is invoked when a given session update request was successful.
        /// Event Side: dApp
        /// </summary>
        event EventHandler<SessionEvent> SessionUpdated;

        /// <summary>
        /// This event is invoked when a given session extend request was successful.
        /// Event Side: dApp
        /// </summary>
        event EventHandler<SessionEvent> SessionExtended;

        /// <summary>
        /// This event is invoked when a given session has been pinged
        /// Event Side: dApp & Wallet
        /// </summary>
        event EventHandler<SessionEvent> SessionPinged;

        /// <summary>
        /// This event is invoked whenever a session has been deleted
        /// Event Side: dApp & Wallet
        /// </summary>
        event EventHandler<SessionEvent> SessionDeleted;

        /// <summary>
        /// This event is invoked whenever a session has been rejected
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<SessionStruct> SessionRejected;

        /// <summary>
        /// This event is invoked whenever a session has been approved
        /// Event Side: Wallet
        /// </summary>
        event EventHandler<SessionStruct> SessionApproved; 

        /// <summary>
        /// This event is invoked whenever a pairing is pinged
        /// Event Side: dApp & Wallet
        /// </summary>
        event EventHandler<PairingEvent> PairingPinged;

        /// <summary>
        /// This event is invoked whenever a pairing is deleted
        /// Event Side: dApp & Wallet
        /// </summary>
        event EventHandler<PairingEvent> PairingDeleted;
        
        /// <summary>
        /// Get static event handlers for requests / responses for the given type T, TR. This is similar to
        /// <see cref="IEngine.HandleMessageType{T,TR}"/> but uses EventHandler rather than callback functions
        /// </summary>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        /// <returns>The <see cref="TypedEventHandler{T,TR}"/> managing events for the given types T, TR</returns>
        TypedEventHandler<T, TR> SessionRequestEvents<T, TR>();
        
        /// <summary>
        /// Get all pending session requests as an array
        /// </summary>
        PendingRequestStruct[] PendingSessionRequests { get; }
        
        /// <summary>
        /// Connect (a dApp) with the given ConnectOptions. At a minimum, you must specified a RequiredNamespace. 
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Connection data that includes the session proposal URI as well as a
        /// way to await for a session approval</returns>
        Task<ConnectedData> Connect(ConnectOptions options);

        /// <summary>
        /// Pair (a wallet) with a peer (dApp) using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown.
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The proposal the connecting peer wants to connect using. You must approve or reject
        /// the proposal</returns>
        Task<ProposalStruct> Pair(string uri);

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to approve</param>
        /// <param name="approvedAddresses">An array of address strings to connect to the session</param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        Task<IApprovedData> Approve(ProposalStruct proposalStruct, params string[] approvedAddresses);

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/> to generate an
        /// <see cref="ApproveParams"/> object, or use the alias function <see cref="IEngineAPI.Approve(ProposalStruct, string[])"/>
        /// </summary>
        /// <param name="params">Parameters for the approval. This usually comes from <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/></param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        Task<IApprovedData> Approve(ApproveParams @params);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.RejectProposal(string)"/> or <see cref="ProposalStruct.RejectProposal(Error)"/>
        /// to generate a <see cref="RejectParams"/> object, or use the alias function <see cref="IEngineAPI.Reject(ProposalStruct, string)"/>
        /// </summary>
        /// <param name="params">The parameters of the rejection</param>
        /// <returns></returns>
        Task Reject(RejectParams @params);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="message">A message explaining the reason for the rejection</param>
        Task Reject(ProposalStruct proposalStruct, string message = null);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="error">An error explaining the reason for the rejection</param>
        Task Reject(ProposalStruct proposalStruct, Error error);

        /// <summary>
        /// Update a session, adding/removing additional namespaces in the given topic.
        /// </summary>
        /// <param name="topic">The topic to update</param>
        /// <param name="namespaces">The updated namespaces</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the updates</returns>
        Task<IAcknowledgement> UpdateSession(string topic, Namespaces namespaces);

        /// <summary>
        /// Extend a session in the given topic. 
        /// </summary>
        /// <param name="topic">The topic of the session to extend</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the extension</returns>
        Task<IAcknowledgement> Extend(string topic);

        /// <summary>
        /// Send a request to the session in the given topic with the request data T. You may (optionally) specify
        /// a chainId the request should be performed in. This function will await a response of type TR from the session.
        ///
        /// If no response is ever received, then a Timeout exception may be thrown.
        ///
        /// The type T MUST define the RpcMethodAttribute to tell the SDK what JSON RPC method to use for the given
        /// type T.
        /// Either type T or TR MUST define a RpcRequestOptions and RpcResponseOptions attribute to tell the SDK
        /// what options to use for the Request / Response.
        /// </summary>
        /// <param name="topic">The topic of the session to send the request in</param>
        /// <param name="data">The data of the request</param>
        /// <param name="chainId">An (optional) chainId the request should be performed in</param>
        /// <param name="expiry">An override to specify how long this request will live for. If null is given, then expiry will be taken from either T or TR attributed options</param>
        /// <typeparam name="T">The type of the request data. MUST define the RpcMethodAttribute</typeparam>
        /// <typeparam name="TR">The type of the response data.</typeparam>
        /// <returns>The response data as type TR</returns>
        Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null);

        /// <summary>
        /// Send a response to a request to the session in the given topic with the response data TR. This function
        /// can be called directly, however it may be easier to use <see cref="TypedEventHandler{T, TR}.OnResponse"/> event
        /// to handle sending responses to specific requests. 
        /// </summary>
        /// <param name="topic">The topic of the session to respond in</param>
        /// <param name="response">The JSON RPC response to send</param>
        /// <typeparam name="T">The type of the request data</typeparam>
        /// <typeparam name="TR">The type of the response data</typeparam>
        Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response);

        /// <summary>
        /// Emit an event to the session with the given topic with the given <see cref="EventData{T}"/>. You may
        /// optionally specify a chainId to specify where the event occured. 
        /// </summary>
        /// <param name="topic">The topic of the session to emit the event to</param>
        /// <param name="eventData">The event data for the event emitted</param>
        /// <param name="chainId">An (optional) chainId to specify where the event occured</param>
        /// <typeparam name="T">The type of the event data</typeparam>
        Task Emit<T>(string topic, EventData<T> eventData, string chainId = null);

        /// <summary>
        /// Send a ping to the session in the given topic
        /// </summary>
        /// <param name="topic">The topic of the session to send a ping to</param>
        Task Ping(string topic);

        /// <summary>
        /// Disconnect a session in the given topic with an (optional) error reason
        /// </summary>
        /// <param name="topic">The topic of the session to disconnect</param>
        /// <param name="reason">An (optional) error reason for the disconnect</param>
        Task Disconnect(string topic, Error reason = null);

        /// <summary>
        /// Find all sessions that have a namespace that match the given <see cref="RequiredNamespaces"/>
        /// </summary>
        /// <param name="requiredNamespaces">The required namespaces the session must have to be returned</param>
        /// <returns>All sessions that have a namespace that match the given <see cref="RequiredNamespaces"/></returns>
        SessionStruct[] Find(RequiredNamespaces requiredNamespaces);

        Task<DisposeHandlerToken> HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback,
            Func<string, JsonRpcResponse<bool>, Task> responseCallback);
        
        /// <summary>
        /// Update the default session, adding/removing additional namespaces in the given topic. The default session
        /// is grabbed from Client.AddressProvider.DefaultSession
        /// </summary>
        /// <param name="namespaces">The updated namespaces</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the updates</returns>
        Task<IAcknowledgement> UpdateSession(Namespaces namespaces);

        /// <summary>
        /// Extend the default session. 
        /// </summary>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the extension</returns>
        Task<IAcknowledgement> Extend();

        /// <summary>
        /// Send a request to the default session with the request data T. You may (optionally) specify
        /// a chainId the request should be performed in. This function will await a response of type TR from the session.
        ///
        /// If no response is ever received, then a Timeout exception may be thrown.
        ///
        /// The type T MUST define the RpcMethodAttribute to tell the SDK what JSON RPC method to use for the given
        /// type T.
        /// Either type T or TR MUST define a RpcRequestOptions and RpcResponseOptions attribute to tell the SDK
        /// what options to use for the Request / Response.
        /// </summary>
        /// <param name="data">The data of the request</param>
        /// <param name="chainId">An (optional) chainId the request should be performed in</param>
        /// <param name="expiry">An override to specify how long this request will live for. If null is given, then expiry will be taken from either T or TR attributed options</param>
        /// <typeparam name="T">The type of the request data. MUST define the RpcMethodAttribute</typeparam>
        /// <typeparam name="TR">The type of the response data.</typeparam>
        /// <returns>The response data as type TR</returns>
        Task<TR> Request<T, TR>(T data, string chainId = null, long? expiry = null);

        /// <summary>
        /// Send a response to a request to the default session with the response data TR. This function
        /// can be called directly, however it may be easier to use <see cref="TypedEventHandler{T, TR}.OnResponse"/> event
        /// to handle sending responses to specific requests. 
        /// </summary>
        /// <param name="response">The JSON RPC response to send</param>
        /// <typeparam name="T">The type of the request data</typeparam>
        /// <typeparam name="TR">The type of the response data</typeparam>
        Task Respond<T, TR>(JsonRpcResponse<TR> response);

        /// <summary>
        /// Emit an event to the default session with the given <see cref="EventData{T}"/>. You may
        /// optionally specify a chainId to specify where the event occured. 
        /// </summary>
        /// <param name="eventData">The event data for the event emitted</param>
        /// <param name="chainId">An (optional) chainId to specify where the event occured</param>
        /// <typeparam name="T">The type of the event data</typeparam>
        Task Emit<T>(EventData<T> eventData, string chainId = null);

        /// <summary>
        /// Send a ping to the default session
        /// </summary>
        Task Ping();

        /// <summary>
        /// Disconnect the default session with an (optional) error reason
        /// </summary>
        /// <param name="reason">An (optional) error reason for the disconnect</param>
        Task Disconnect(Error reason = null);
    }
}
