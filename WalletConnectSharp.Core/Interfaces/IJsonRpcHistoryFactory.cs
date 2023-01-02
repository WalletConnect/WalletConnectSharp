using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IJsonRpcHistoryFactory
    {
        ICore Core { get; }
        
        Task<IJsonRpcHistory<T, TR>> JsonRpcHistoryOfType<T, TR>();
    }
}