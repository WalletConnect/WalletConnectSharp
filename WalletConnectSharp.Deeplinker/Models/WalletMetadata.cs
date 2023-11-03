using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class WalletMetadata
{
    [JsonProperty("shortName")]
    public string ShortName;

    [JsonProperty("colors")]
    public WalletColors Colors;
}
