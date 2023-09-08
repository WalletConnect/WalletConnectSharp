namespace WalletConnectSharp.Common.Logging
{
    public class WCLogger
    {
        public static ILogger Logger;

        public static void Log(string message)
        {
            if (Logger == null)
                return;
            
            Logger.Log(message);
        }

        public static void LogError(string message)
        {
            if (Logger == null)
                return;
            
            Logger.LogError(message);
        }

        public static void LogError(Exception e)
        {
            if (Logger == null)
                return;
            
            Logger.LogError(e);
        }
    }
}
