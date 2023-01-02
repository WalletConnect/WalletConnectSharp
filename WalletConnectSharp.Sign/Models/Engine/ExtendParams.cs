using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class ExtendParams
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}