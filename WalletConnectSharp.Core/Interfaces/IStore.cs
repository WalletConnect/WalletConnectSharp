using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IStore<TKey, TValue> : IModule where TValue : IKeyHolder<TKey>
    {
        public int Length { get; }
        
        public TKey[] Keys { get; }
        
        public TValue[] Values { get; }

        public Task Init();

        public Task Set(TKey key, TValue value);

        public TValue Get(TKey key);

        public Task Update(TKey key, TValue update);

        public Task Delete(TKey key, ErrorResponse reason);
    }
}