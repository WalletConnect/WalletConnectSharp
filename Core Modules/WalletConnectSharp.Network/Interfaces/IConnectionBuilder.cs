namespace WalletConnectSharp.Network.Interfaces
{
    public interface IConnectionBuilder
    {
        Task<IJsonRpcConnection> CreateConnection(string url);
    }
}
