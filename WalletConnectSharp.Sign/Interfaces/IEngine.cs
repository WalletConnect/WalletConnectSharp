using System;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents the Engine for running the Sign client. This interface
    /// is an sub-type of <see cref="IEngineAPI"/> and represents the actual Engine. This is
    /// different than the Sign client.
    /// </summary>
    public interface IEngine : IEngineAPI
    {
        /// <summary>
        /// The <see cref="ISignClient"/> this Engine is using
        /// </summary>
        ISignClient Client { get; }

        /// <summary>
        /// Initialize the Engine. This loads any persistant state and connects to the WalletConnect
        /// relay server 
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Handle a specific request / response type and call the given callbacks for requests and responses. The
        /// response callback is only triggered when it originates from the request of the same type.
        /// </summary>
        /// <param name="requestCallback">The callback function to invoke when a request is received with the given request type</param>
        /// <param name="responseCallback">The callback function to invoke when a response is received with the given response type</param>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        void HandleMessageType<T, TR>(Func<string, JsonRpcRequest<T>, Task> requestCallback,
            Func<string, JsonRpcResponse<TR>, Task> responseCallback);

        /// <summary>
        /// Get static event handlers for requests / responses for the given type T, TR. This is similar to
        /// <see cref="IEngine.HandleMessageType{T,TR}"/> but uses EventHandler rather than callback functions
        /// </summary>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        /// <returns>The <see cref="TypedEventHandler{T,TR}"/> managing events for the given types T, TR</returns>
        TypedEventHandler<T, TR> SessionRequestEvents<T, TR>();

        /// <summary>
        /// Parse a session proposal URI and return all information in the URI. 
        /// </summary>
        /// <param name="uri">The URI to parse</param>
        /// <returns>The parameters parsed from the URI</returns>
        UriParameters ParseUri(string uri);

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in either type</exception>
        PublishOptions RpcRequestOptionsFromType<T1, T2>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in the type T</exception>
        PublishOptions RpcRequestOptionsForType<T>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in either type</exception>
        PublishOptions RpcResponseOptionsFromTypes<T1, T2>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in the type T</exception>
        PublishOptions RpcResponseOptionsForType<T>();
    }
}
