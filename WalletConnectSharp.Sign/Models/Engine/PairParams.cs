using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public class PairParams
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}