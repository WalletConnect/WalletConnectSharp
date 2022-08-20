namespace WalletConnectSharp.Core.Models;

public class WCSessionUpdate : JsonRpcRequest
{
    public override string Method
    {
        get { return WalletConnectStates.SessionUpdate; }
    }

    [JsonProperty("params")]
    public WCSessionData[] parameters;

    public WCSessionUpdate(WCSessionData data)
    {
        this.parameters = new[] { data };
    }
}
