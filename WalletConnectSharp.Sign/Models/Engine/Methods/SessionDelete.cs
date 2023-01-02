using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionDelete")]
    [RpcRequestOptions(Clock.ONE_DAY, false, 1112)]
    [RpcResponseOptions(Clock.ONE_DAY, false, 1113)]
    public class SessionDelete : ErrorResponse, IWcMethod
    {
    }
}