using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionExtend")]
    [RpcRequestOptions(Clock.ONE_DAY, false, 1106)]
    [RpcResponseOptions(Clock.ONE_DAY, false, 1107)]
    public class SessionExtend : Dictionary<string, object>, IWcMethod
    {
    }
}