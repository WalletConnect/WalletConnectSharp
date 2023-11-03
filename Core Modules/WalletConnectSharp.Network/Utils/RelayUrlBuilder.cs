using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Interfaces;
using WalletConnectSharp.Platform;

namespace WalletConnectSharp.Network;

public class RelayUrlBuilder : IRelayUrlBuilder
{
    public virtual string FormatRelayRpcUrl(string relayUrl, string protocol, string version, string projectId,
        string auth)
    {
        var splitUrl = relayUrl.Split("?");
        var ua = BuildUserAgent(protocol, version);

        var currentParameters = UrlUtils.ParseQs(splitUrl.Length > 1 ? splitUrl[1] : "");

        currentParameters.Add("auth", auth);
        currentParameters.Add("projectId", projectId);
        currentParameters.Add("ua", ua);

        var hasOrigin = TryGetOrigin(out var origin);
        if (hasOrigin)
            currentParameters.Add("origin", origin);

        var formattedParameters = UrlUtils.StringifyQs(currentParameters);

        return splitUrl[0] + formattedParameters;
    }

    public virtual string BuildUserAgent(string protocol, string version)
    {
        var (os, osVersion) = DevicePlatform.GetOsInfo();
        var (sdkName, sdkVersion) = DevicePlatform.GetSdkInfo();

        return $"{protocol}-{version}/{sdkName}-{sdkVersion}/{os}-{osVersion}";
    }

    protected virtual bool TryGetOrigin(out string origin)
    {
        origin = null;
        return false;
    }
}
