using Newtonsoft.Json;
using WalletConnectSharp.Core;

namespace WalletConnectSharp.Auth.Models;

public class Requester
{
    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; }
    
    [JsonProperty("publicKey")]
    public string PublicKey { get; set; }
}
