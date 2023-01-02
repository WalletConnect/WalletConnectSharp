using System;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Controllers
{
    public class Pairing : Store<string, PairingStruct>, IPairing
    {
        public Pairing(ICore core) : base(core, "pairing", WalletConnectSignClient.StoragePrefix)
        {
        }
    }
}