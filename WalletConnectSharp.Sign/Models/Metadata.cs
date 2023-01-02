using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class Metadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("icons")]
        public string[] Icons { get; set; }
    }
}