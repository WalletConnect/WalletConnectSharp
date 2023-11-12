using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectSharp.Platform;

public static class DevicePlatform
{
    private static IPlatform? _backend;

    public static IPlatform Backend
    {
        get
        {
            return _backend ?? throw new InvalidOperationException("No backend platform");
        }
        set
        {
            _backend = value ?? throw new InvalidOperationException("Backend platform must be non-null");
        }
    }

    public static (string name, string version) GetOsInfo() => Backend.GetOsInfo();

    public static (string name, string version) GetSdkInfo() => Backend.GetSdkInfo();

    public static IRelayUrlBuilder RelayUrlBuilder
    {
        get
        {
            return Backend.RelayUrlBuilder;
        }
        set
        {
            Backend.RelayUrlBuilder = value;
        }
    }

    public static IConnectionBuilder ConnectionBuilder
    {
        get
        {
            return Backend.ConnectionBuilder;
        }
        set
        {
            Backend.ConnectionBuilder = value;
        }
    }
    public static Task OpenUrl(string url) => Backend.OpenUrl(url);
}
