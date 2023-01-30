using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    public class EventData<T>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
