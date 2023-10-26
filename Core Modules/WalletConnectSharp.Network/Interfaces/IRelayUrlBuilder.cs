namespace WalletConnectSharp.Network
{
    public interface IRelayUrlBuilder
    {
        public string FormatRelayRpcUrl(string relayUrl, string protocol, string version, string projectId,
            string auth);

        public string BuildUserAgent(string protocol, string version);

        public (string name, string version) GetOsInfo();

        public (string name, string version) GetSdkInfo();
    }
}
