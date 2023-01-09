using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.History;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// A module that stores Json RPC request/response history data for a given Request type (T) and Response type (TR).
    /// Each request / response is stored in a JsonRpcRecord of type T, TR
    /// </summary>
    /// <typeparam name="T">The JSON RPC Request type</typeparam>
    /// <typeparam name="TR">The JSON RPC Response type</typeparam>
    public interface IJsonRpcHistory<T, TR> : IModule, IEvents
    {
        /// <summary>
        /// A mapping of Json RPC Records to their corresponding Json RPC id
        /// </summary>
        IReadOnlyDictionary<long, JsonRpcRecord<T, TR>> Records { get; }
        
        /// <summary>
        /// The number of history records stored
        /// </summary>
        int Size { get; }
        
        /// <summary>
        /// An array of all JsonRpcRecord ids
        /// </summary>
        long[] Keys { get; }
        
        /// <summary>
        /// An array of all JsonRpcRecords, each record contains a request / response
        /// </summary>
        JsonRpcRecord<T, TR>[] Values { get; }
        
        /// <summary>
        /// An array of all pending requests. A request is pending when it has no response
        /// </summary>
        RequestEvent<T>[] Pending { get; }

        /// <summary>
        /// Initialize this JsonRpcFactory. This will restore all history records from storage
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Set a new request in the given topic on the given chainId. This will add the request to the
        /// history as pending. To add a response to this request, use the Resolve method
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="request"></param>
        /// <param name="chainId"></param>
        void Set(string topic, IJsonRpcRequest<T> request, string chainId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<JsonRpcRecord<T, TR>> Get(string topic, long id);

        Task Resolve(IJsonRpcResult<TR> response);

        void Delete(string topic, long? id);

        Task<bool> Exists(string topic, long id);
    }
}
