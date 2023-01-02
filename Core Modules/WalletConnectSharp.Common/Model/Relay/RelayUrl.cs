using System;

namespace WalletConnectSharp.Common.Utils
{
    public static class RelayUrl
    {
        public static string FormatRelayRpcUrl(string protocol, string version, string relayUrl, string sdkVersion,
            string projectId, string auth)
        {
            var splitUrl = relayUrl.Split("?");
            var ua = FormatUA(protocol, version, sdkVersion);

            var currentParameters = UrlUtils.ParseQs(splitUrl.Length > 1 ? splitUrl[1] : "");
            
            currentParameters.Add("auth", auth);
            currentParameters.Add("projectId", projectId);
            currentParameters.Add("ua", ua);

            var formattedParameters = UrlUtils.StringifyQs(currentParameters);

            return splitUrl[0] + formattedParameters;
        }
        
        public static string FormatUA(string protocol, string version, string sdkVersion)
        {
            var os = Environment.OSVersion.Platform.ToString();
            var osVersion = Environment.OSVersion.Version.ToString();

            var osInfo = string.Join("-", os, osVersion);
            var environment = "WalletConnectSharpv2:" + Environment.Version;

            var sdkType = "C#";

            return string.Join("/", string.Join("-", protocol, version), string.Join("-", sdkType, sdkVersion), osInfo,
                environment);
        }
    }
}