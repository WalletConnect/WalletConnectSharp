using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Interfaces
{
    public interface IKeyHolder<TKey>
    {
        [JsonIgnore]
        public TKey Key { get; }
    }
}