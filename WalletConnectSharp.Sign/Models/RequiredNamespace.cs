using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class RequiredNamespace : BaseRequiredNamespace
    {
        [JsonProperty("extension", NullValueHandling=NullValueHandling.Ignore)]
        public BaseRequiredNamespace[] Extension { get; set; }
    }
}