using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A class that holds request arguments that can be
    /// placed inside a Json RPC request. Does not represent
    /// a full Json RPC request
    /// </summary>
    /// <typeparam name="T">The type of the parameter in this request</typeparam>
    public class RequestArguments<T> : IRequestArguments<T>
    {
        [JsonProperty("method")]
        private string _method;
        
        [JsonProperty("params")]
        private T _params;

        /// <summary>
        /// The method to use
        /// </summary>
        [JsonIgnore]
        public string Method
        {
            get => _method;
            set => _method = value;
        }

        /// <summary>
        /// The method for this request
        /// </summary>
        [JsonIgnore]
        public T Params
        {
            get => _params;
            set => _params = value;
        }
    }
}
