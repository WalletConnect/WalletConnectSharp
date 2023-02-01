using System.Collections.Generic;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A dictionary of RequiredNamespace based on a chainId key. The chainId
    /// should follow CAIP-2 format
    /// chain_id:    namespace + ":" + reference
    /// namespace:   [-a-z0-9]{3,8}
    /// reference:   [-_a-zA-Z0-9]{1,32}
    /// </summary>
    public class RequiredNamespaces : Dictionary<string, RequiredNamespace> { }
}
