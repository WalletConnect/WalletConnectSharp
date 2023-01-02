using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    public interface IJsonRpcValidationResult
    {
        [JsonProperty("valid")]
        bool Valid { get; }
        
        [JsonProperty("error")]
        string Error { get; }
    }
}