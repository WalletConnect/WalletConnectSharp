using Newtonsoft.Json;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models.History
{
    public class JsonRpcRecord<T, R>
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("request")]
        public IRequestArguments<T> Request { get; set; }
        
        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("response")]
        public IJsonRpcResult<R> Response;

        /// <summary>
        /// This constructor is required for the JSON deserializer to be able
        /// to identify concrete classes to use when deserializing the interface properties.
        /// </summary>
        public JsonRpcRecord(IJsonRpcRequest<T> request)
        {
            Request = request;
        }
    }
}