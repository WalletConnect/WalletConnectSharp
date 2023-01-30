using System;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Controllers
{
    /// <summary>
    /// A <see cref="Store{TKey,TValue}"/> module for storing
    /// <see cref="PairingStruct"/> data. This will be used
    /// for storing pairing data
    /// </summary>
    public class Pairing : Store<string, PairingStruct>, IPairing
    {
        /// <summary>
        /// Create a new instance of this module
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> instance that will be used for <see cref="ICore.Storage"/></param>
        public Pairing(ICore core) : base(core, "pairing", WalletConnectSignClient.StoragePrefix)
        {
        }
    }
}
