using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Models;

public class JsonRpcRequest : IEventSource
{
    [JsonProperty(Order = 1)]
    private long id;
    [JsonProperty(Order = 2)]
    private string jsonrpc = "2.0";

    [JsonProperty("method", Order = 3)]
    public virtual string Method { get; protected set; }

    public JsonRpcRequest()
    {
        if (this.id == 0)
        {
            this.id = RpcPayloadId.Generate();
        }
    }

    [JsonIgnore]
    public long ID
    {
        get { return id; }
    }

    [JsonIgnore]
    public string JsonRPC
    {
        get { return jsonrpc; }
    }

    [JsonIgnore]
    public string Event
    {
        get { return Method; }
    }
}
