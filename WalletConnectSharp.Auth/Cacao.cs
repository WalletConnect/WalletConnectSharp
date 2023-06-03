using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Auth.Models;

[RpcResponseOptions(Clock.ONE_MINUTE, 3001)]
public class Cacao : Message
{
    public struct CacaoHeader
    {
        [JsonProperty]
        public readonly string t = "eip4361";

        public CacaoHeader()
        {
        }
    }
    
    public class CacaoRequestPayload
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }
        
        [JsonProperty("aud")]
        public string Aud { get; set; }
        
        [JsonProperty("version")]
        public string Version { get; set; }
        
        [JsonProperty("nonce")]
        public string Nonce { get; set; }
        
        [JsonProperty("iat")]
        public string Iat { get; set; }
        
        [JsonProperty("nbf")]
        public string Nbf { get; set; }
        
        [JsonProperty("exp")]
        public string Exp { get; set; }
        
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("statement")]
        public string Statement { get; set; }
        
        [JsonProperty("requestId")]
        public string RequestId { get; set; }
        
        [JsonProperty("resources")]
        public string[] Resource { get; set; }
    }

    public class CacaoPayload : CacaoRequestPayload
    {
        [JsonProperty("iss")]
        public string Iss { get; set; }

        public CacaoPayload(CacaoRequestPayload source)
        {
            this.Aud = source.Aud;
            this.Domain = source.Domain;
            this.Exp = source.Exp;
            this.Iat = source.Iat;
            this.Nbf = source.Nbf;
            this.Nonce = source.Nonce;
            this.Resource = source.Resource;
            this.Statement = source.Statement;
            this.Version = source.Version;
            this.ChainId = source.ChainId;
            this.RequestId = source.RequestId;
        }
        
        public CacaoPayload(CacaoRequestPayload source, string iss)
        {
            this.Aud = source.Aud;
            this.Domain = source.Domain;
            this.Exp = source.Exp;
            this.Iat = source.Iat;
            this.Nbf = source.Nbf;
            this.Nonce = source.Nonce;
            this.Resource = source.Resource;
            this.Statement = source.Statement;
            this.Version = source.Version;
            this.ChainId = source.ChainId;
            this.RequestId = source.RequestId;

            this.Iss = iss;
        }
    }

    public abstract class CacaoSignature
    {
        [JsonProperty("t")]
        public readonly string T;
        
        public class EIP191CacaoSignature : CacaoSignature
        {
            [JsonProperty("t")]
            public readonly string T = "eip191";
        }

        public class EIP1271CacaoSignature : CacaoSignature
        {
            [JsonProperty("t")]
            public readonly string T = "eip1271";
        }
        
        [JsonProperty("s")]
        public string S { get; set; }
        
        [JsonProperty("m")]
        public string M { get; set; }
    }

    [JsonProperty("h")]
    public readonly CacaoHeader Header = new CacaoHeader();
    
    [JsonProperty("p")]
    public CacaoPayload Payload { get; set; }
    
    [JsonProperty("s")]
    public CacaoSignature Signature { get; set; }
}
