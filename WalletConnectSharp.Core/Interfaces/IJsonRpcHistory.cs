using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.History;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IJsonRpcHistory<T, TR> : IModule, IEvents
    {
        IReadOnlyDictionary<long, JsonRpcRecord<T, TR>> Records { get; }
        
        int Size { get; }
        
        long[] Keys { get; }
        
        JsonRpcRecord<T, TR>[] Values { get; }
        
        RequestEvent<T>[] Pending { get; }

        Task Init();

        void Set(string topic, IJsonRpcRequest<T> request, string chainId);

        Task<JsonRpcRecord<T, TR>> Get(string topic, long id);

        Task Resolve(IJsonRpcResult<TR> response);

        void Delete(string topic, long? id);

        Task<bool> Exists(string topic, long id);
    }
}