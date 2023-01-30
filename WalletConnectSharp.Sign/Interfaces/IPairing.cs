using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// A <see cref="IStore{TKey,TValue}"/> interface for a module
    /// that stores <see cref="PairingStruct"/> data.
    /// </summary>
    public interface IPairing : IStore<string, PairingStruct> { }
}
