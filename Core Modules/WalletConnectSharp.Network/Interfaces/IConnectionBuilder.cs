namespace WalletConnectSharp.Network.Interfaces
{
    public interface IConnectionBuilder
    {
        IJsonRpcConnection CreateConnection(string url);
    }
}
