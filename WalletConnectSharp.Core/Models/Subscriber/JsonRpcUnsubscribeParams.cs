using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    /// <summary>
    /// The JSON RPC Request to unsubscribe to a given subscription Id and topic
    /// </summary>
    public class JsonRpcUnsubscribeParams
    {
        /// <summary>
        /// The subscription id to unsubscribe from
        /// </summary>
        [JsonProperty("id")]
        public string Id;
        
        /// <summary>
        /// The topic the subscription exists in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic;
    }
}
