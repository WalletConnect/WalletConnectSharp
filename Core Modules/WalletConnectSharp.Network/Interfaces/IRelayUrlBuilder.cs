namespace WalletConnectSharp.Network.Interfaces
{
    public interface IRelayUrlBuilder
    {
        public string FormatRelayRpcUrl(string relayUrl, string protocol, string version, string projectId,
            string auth);

        public string BuildUserAgent(string protocol, string version);
    }
}
