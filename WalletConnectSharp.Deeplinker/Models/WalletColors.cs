using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class WalletColors
{
    [JsonProperty("primary")]
    public string Primary;

    [JsonProperty("secondary")]
    public string Secondary;
}
