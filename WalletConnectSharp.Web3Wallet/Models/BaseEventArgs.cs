using Newtonsoft.Json;

namespace WalletConnectSharp.Web3Wallet.Models;

public class BaseEventArgs<T> : EventArgs
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    [JsonProperty("topic")]
    public string Topic { get; set; }
    
    [JsonProperty("params")]
    public T Parameters { get; set; }
}
