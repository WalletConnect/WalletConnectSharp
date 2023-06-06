using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Auth.Models;

public class AuthOptions : CoreOptions
{
    public AuthMetadata Metadata { get; set; }
    
    public ICore Core { get; set; }
}
