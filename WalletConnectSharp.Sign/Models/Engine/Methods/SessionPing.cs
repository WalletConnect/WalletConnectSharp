using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionPing")]
    [RpcRequestOptions(Clock.THIRTY_SECONDS, false, 1114)]
    [RpcResponseOptions(Clock.THIRTY_SECONDS, false, 1115)]
    public class SessionPing : Dictionary<string, object>, IWcMethod
    {
    }
}