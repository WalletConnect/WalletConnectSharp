using System.Globalization;

namespace WalletConnectSharp.Auth.Internals
{
    public static class IssDidUtils
    {
        public static string[] ExtractDidAddressSegments(string iss)
        {
            if (string.IsNullOrWhiteSpace(iss))
                return null;

            return iss.Split(":");
        }

        public static string DidChainId(string iss)
        {
            if (string.IsNullOrWhiteSpace(iss))
                return null;

            return ExtractDidAddressSegments(iss)[3];
        }

        public static string NamespacedDidChainId(string iss)
        {
            if (string.IsNullOrWhiteSpace(iss))
                return null;

            var segments = ExtractDidAddressSegments(iss);

            return $"{segments[2]}:{segments[3]}";
        }

        public static string DidAddress(string iss)
        {
            if (string.IsNullOrWhiteSpace(iss))
                return null;

            var segments = ExtractDidAddressSegments(iss);

            if (segments.Length == 0)
                return null;

            return segments[segments.Length - 1];
        }

        public static string ToISOString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
        }
    }
}
