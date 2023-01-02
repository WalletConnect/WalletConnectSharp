using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class FindParams
    {
        [JsonProperty("requiredNamespaces")]
        public RequiredNamespaces RequiredNamespaces { get; set; }
    }
}