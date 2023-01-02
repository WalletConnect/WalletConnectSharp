namespace WalletConnectSharp.Network.Models
{
    public class RequestArguments<T> : IRequestArguments<T>
    {
        public string Method { get; set; }
        public T Params { get; set; }
    }
}