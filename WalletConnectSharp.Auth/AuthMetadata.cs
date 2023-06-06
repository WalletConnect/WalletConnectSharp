using Newtonsoft.Json;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Core;

namespace WalletConnectSharp.Auth;

public class AuthMetadata : Metadata
{
    [JsonProperty("redirect")]
    public RedirectData Redirect { get; set; }
    
    [JsonProperty("verifyUrl")]
    public string VerifyUrl { get; set; }
}
