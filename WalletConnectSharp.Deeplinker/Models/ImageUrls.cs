using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class ImageUrls
{
    [JsonProperty("sm")] public string SmallUrl;

    [JsonProperty("md")] public string MediumUrl;

    [JsonProperty("lg")] public string LargeUrl;
}
