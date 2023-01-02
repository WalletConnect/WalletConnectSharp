using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class Namespace : BaseNamespace
    {
        [JsonProperty("extension", NullValueHandling=NullValueHandling.Ignore)]
        public BaseNamespace[] Extension { get; set; }
    }
}