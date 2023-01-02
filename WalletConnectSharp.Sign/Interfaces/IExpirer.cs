using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Sign.Models.Expirer;

namespace WalletConnectSharp.Sign.Interfaces
{
    public interface IExpirer : IModule, IEvents
    {
        int Length { get; }
        
        string[] Keys { get; }
        
        Expiration[] Values { get; }

        Task Init();

        bool Has(string key);

        bool Has(long key);

        void Set(string key, long expiry);

        void Set(long key, long expiry);

        Expiration Get(string key);

        Expiration Get(long key);

        Task Delete(string key);

        Task Delete(long key);
    }
}