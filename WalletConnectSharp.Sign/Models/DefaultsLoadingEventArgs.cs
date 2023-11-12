using WalletConnectSharp.Sign.Controllers;

namespace WalletConnectSharp.Sign.Models;

public class DefaultsLoadingEventArgs : EventArgs
{
    public DefaultsLoadingEventArgs(ref AddressProvider.DefaultData defaults)
    {
        Defaults = defaults;
    }

    public AddressProvider.DefaultData Defaults { get; }
}
