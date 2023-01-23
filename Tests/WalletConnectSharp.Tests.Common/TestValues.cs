namespace WalletConnectSharp.Tests.Common
{
    public static class TestValues
    {
        private const string DefaultProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c";
        private static readonly string EnvironmentProjectId = Environment.GetEnvironmentVariable("PROJECT_ID");

        public static readonly string TestProjectId = !string.IsNullOrWhiteSpace(EnvironmentProjectId)
            ? EnvironmentProjectId
            : DefaultProjectId;

        private const string DefaultRelayUrl = "wss://relay.walletconnect.com";
        private static readonly string EnvironmentRelayUrl = Environment.GetEnvironmentVariable("RELAY_ENDPOINT");
        public static readonly string TestRelayUrl = !string.IsNullOrWhiteSpace(EnvironmentRelayUrl) ? EnvironmentRelayUrl : DefaultRelayUrl;
    }
}
