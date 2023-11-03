using Newtonsoft.Json;

namespace WalletConnectSharp.Deeplinker.Models;

public class WalletInjectionData
{
    [JsonProperty("namespace")]
    public string Namespace;

    [JsonProperty("injected_id")]
    public string InjectedId;
}
