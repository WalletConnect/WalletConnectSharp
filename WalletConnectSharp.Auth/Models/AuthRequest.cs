using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Verify;

namespace WalletConnectSharp.Auth.Models;

public class AuthRequest : TopicMessage
{
    [JsonProperty("params")]
    public AuthRequestData Parameters { get; set; }
    
    [JsonProperty("verifyContext")]
    public VerifiedContext VerifyContext { get; set; }
}
