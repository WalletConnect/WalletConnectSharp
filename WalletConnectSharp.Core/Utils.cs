namespace WalletConnectSharp.Core;

public static class Utils
{
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
            
        try
        {
            new Uri(url);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static bool IsValidRequestExpiry(long expiry, long min, long max)
    {
        return expiry <= max && expiry >= min;
    }
}
