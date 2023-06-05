using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class RequestParams : AuthPayload
{
    [JsonProperty("expiry", NullValueHandling = NullValueHandling.Ignore)]
    public long? Expiry { get; set; }

    public RequestParams() { }

    public RequestParams(AuthPayload payload)
    {
        this.Aud = payload.Aud;
        this.Domain = payload.Domain;
        this.Exp = payload.Exp;
        this.Nbf = payload.Nbf;
        this.Nonce = payload.Nonce;
        this.Resources = payload.Resources;
        this.Statement = payload.Statement;
        this.Type = payload.Type;
        this.ChainId = payload.ChainId;
        this.RequestId = payload.RequestId;
    }
}
