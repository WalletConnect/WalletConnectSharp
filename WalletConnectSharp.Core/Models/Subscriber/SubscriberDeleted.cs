using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class SubscriberDeleted : SubscriberActive
    {
        [JsonProperty("reason")]
        public ErrorResponse Reason { get; set; }
    }
}