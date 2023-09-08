using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Auth.Models;

public class AuthOptions : CoreOptions
{
    public Metadata Metadata;
    
    public ICore Core { get; set; }
}
