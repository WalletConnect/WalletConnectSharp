using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Web3Wallet.Models;

public class SessionRequestEventArgs<T> : BaseEventArgs<SessionRequest<T>>
{
    [JsonProperty("verifyContext")]
    public VerifiedContext VerifyContext;
}
