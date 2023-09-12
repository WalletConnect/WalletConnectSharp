using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models
{
    [Serializable]
    public class RedirectData
    {
        [JsonProperty("native")] public string Native;

        [JsonProperty("universal")] public string Universal;
    }
}
