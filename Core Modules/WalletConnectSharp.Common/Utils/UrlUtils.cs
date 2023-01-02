using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WalletConnectSharp.Common.Utils
{
    public static class UrlUtils
    {
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
    }
}