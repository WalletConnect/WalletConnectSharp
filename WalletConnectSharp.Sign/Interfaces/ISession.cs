using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces
{
    public interface ISession : IStore<string, SessionStruct> { }
}