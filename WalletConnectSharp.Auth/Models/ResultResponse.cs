using Newtonsoft.Json;

namespace WalletConnectSharp.Auth.Models;

public class ResultResponse : Message
{
    [JsonProperty("signature")]
    public Cacao.CacaoSignature Signature;
}
