using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models;

public class BatchFetchMessageRequest
{
    [JsonProperty("topics")]
    public string[] Topics;
}
