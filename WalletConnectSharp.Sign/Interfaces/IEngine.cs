using System;
using System.Threading.Tasks;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents the Engine for running the Sign client. This interface
    /// is an sub-type of <see cref="IEngineTasks"/> and represents the actual Engine. This is
    /// different than the Sign client.
    /// </summary>
    public interface IEngine : IEngineTasks
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
    }
}
