namespace WalletConnectSharp.Common.Utils
{
    public static class Constants
    {
        public const string CORE_PROTOCOL = "wc";
        public const int CORE_VERSION = 2;
        public const string CORE_CONTEXT = "core";

        public static readonly string CORE_STORAGE_PREFIX = CORE_PROTOCOL + "@" + CORE_VERSION + ":" + CORE_CONTEXT + ":";
        
        
    }
}