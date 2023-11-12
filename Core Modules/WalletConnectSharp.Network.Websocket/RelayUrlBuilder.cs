using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WalletConnectSharp.Network.Interfaces;
using WalletConnectSharp.Platform;

namespace WalletConnectSharp.Network;

public class RelayUrlBuilder : IRelayUrlBuilder
{
    /// <summary>
    /// Parse query strings encoded parameters and return a NameValueCollection
    /// </summary>
    /// <param name="queryString">The query string to parse</param>
    /// <returns>The NameValueCollection containing all parameters in the query parameter</returns>
    public static NameValueCollection ParseQs(string queryString)
    {
        return Regex.Matches(queryString, "([^?=&]+)(=([^&]*))?")
            .ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value)
            .Aggregate(new NameValueCollection(), (seed, current) =>
            {
                seed.Add(current.Key, current.Value);
                return seed;
            });
    }

    /// <summary>
    /// Convert a NameValueCollection to a query string
    /// </summary>
    /// <param name="params">The NameValueCollection to convert to a query string</param>
    /// <returns>A query string containing all parameters from the NameValueCollection</returns>
    public static string StringifyQs(NameValueCollection @params)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var key in @params.AllKeys)
        {
            var values = @params.GetValues(key);
            if (values == null)
                continue;
                
            foreach (var value in values)
            {
                sb.Append(sb.Length == 0 ? "?" : "&");
                sb.AppendFormat("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
            }
        }

        return sb.ToString();
    }
    
    public virtual string FormatRelayRpcUrl(string relayUrl, string protocol, string version, string projectId,
        string auth)
    {
        var splitUrl = relayUrl.Split("?");
        var ua = BuildUserAgent(protocol, version);

        var currentParameters = ParseQs(splitUrl.Length > 1 ? splitUrl[1] : "");

        currentParameters.Add("auth", auth);
        currentParameters.Add("projectId", projectId);
        currentParameters.Add("ua", ua);

        var hasOrigin = TryGetOrigin(out var origin);
        if (hasOrigin)
            currentParameters.Add("origin", origin);

        var formattedParameters = StringifyQs(currentParameters);

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
