using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Heartbeat
{
    public class HeartBeatOptions
    {
        [JsonProperty("interval")]
        public int Interval { get; set; }
    }
}