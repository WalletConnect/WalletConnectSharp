using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Eth;

public class EthCall
{
    [JsonProperty("to")] public string To;

    [JsonProperty("data")] public string Data;
}
