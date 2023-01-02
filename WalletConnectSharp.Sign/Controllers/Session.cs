using System;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Controllers
{
    public class Session : Store<string, SessionStruct>, ISession
    {
        public Session(ICore core) : base(core, "session", WalletConnectSignClient.StoragePrefix)
        {
        }
    }
}