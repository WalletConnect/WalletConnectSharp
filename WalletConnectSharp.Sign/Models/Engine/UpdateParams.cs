using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class UpdateParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
    }
}