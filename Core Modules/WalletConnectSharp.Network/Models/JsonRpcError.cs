using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Models
{
    public class JsonRpcError : IJsonRpcError
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

        public JsonRpcError()
        {
        }

        public JsonRpcError(long id, ErrorResponse error)
        {
            Id = id;
            Error = error;
        }
    }
}