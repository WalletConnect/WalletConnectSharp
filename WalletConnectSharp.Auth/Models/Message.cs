using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Auth.Models;

public class Message : IKeyHolder<long>
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id;

    public long Key
    {
        get
        {
            if (Id != null)
                return (long)Id;
            throw new KeyNotFoundException("Id Key for message instance is null: " + this.ToString());
        }
    }
}
