using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class WalletLinkData
{
    [JsonProperty("native")]
    public string NativeProtocol;

    [JsonProperty("universal")]
    public string UniversalUrl;
}
