using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class RequestParams : AuthPayload
{
    [JsonProperty("expiry", NullValueHandling = NullValueHandling.Ignore)]
    public long? Expiry { get; set; }
}
