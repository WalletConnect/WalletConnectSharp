using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IJsonRpcProviderMessage<T>
    {
        [JsonProperty("type")]
        string Type { get; }
        
        [JsonProperty("data")]
        T Data { get; }
    }
}