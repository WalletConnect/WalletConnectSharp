using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    [RpcMethod("wc_sessionRequest")]
    [RpcRequestOptions(Clock.ONE_DAY, true, 1108)]
    [RpcResponseOptions(Clock.ONE_DAY, false, 1109)]
    public class SessionRequest<T> : IWcMethod
    {
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("request")]
        public JsonRpcRequest<T> Request { get; set; }
    }
}