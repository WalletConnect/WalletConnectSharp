namespace WalletConnectSharp.Common.Logging;

public class WrapperLogger : ILogger
{
    private ILogger _logger;
    private string prefix;

    public WrapperLogger(ILogger logger, string prefix)
    {
        _logger = logger;
        this.prefix = prefix;
    }

    public void Log(string message)
    {
        _logger.Log($"[{prefix}] {message}");
    }

    public void LogError(string message)
    {
        _logger.LogError($"[{prefix}] {message}");
    }

    public void LogError(Exception e)
    {
        _logger.LogError(e);
    }
}
