using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Represents a full JSON RPC response with the given result type of T
    /// </summary>
    /// <typeparam name="T">The type of the result property for this JSON RPC response</typeparam>
    public class JsonRpcResponse<T> : IJsonRpcResult<T>
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string JsonRPC
        {
            get
            {
                return "2.0";
            }
        }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorResponse Error { get; set; }
        
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }

        [JsonIgnore]
        public bool IsError
        {
            get
            {
                return Error != null;
            }
        }

        public JsonRpcResponse()
        {
        }

        public JsonRpcResponse(long id, ErrorResponse error, T result)
        {
            Id = id;
            Error = error;
            Result = result;
        }
    }
}