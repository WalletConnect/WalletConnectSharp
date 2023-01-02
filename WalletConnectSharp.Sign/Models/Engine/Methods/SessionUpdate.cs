using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionUpdate")]
    [RpcRequestOptions(Clock.ONE_DAY, false, 1104)]
    [RpcResponseOptions(Clock.ONE_DAY, false, 1105)]
    public class SessionUpdate : IWcMethod
    {
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
    }
}