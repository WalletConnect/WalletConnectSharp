using System.Collections.Generic;
using Newtonsoft.Json;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models.History
{
    public class RequestEvent<T>
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("request")]
        public IRequestArguments<T> Request { get; set; }
        
        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        public static RequestEvent<T> FromPending<TR>(JsonRpcRecord<T, TR> pending)
        {
            return new RequestEvent<T>()
            {
                Topic = pending.Topic,
                Request = pending.Request,
                ChainId = pending.ChainId,
            };
        }
    }
}