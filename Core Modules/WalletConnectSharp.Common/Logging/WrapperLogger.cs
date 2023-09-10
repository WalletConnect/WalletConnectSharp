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
        if (_logger == null) return;
        _logger.Log($"[{prefix}] {message}");
    }

    public void LogError(string message)
    {
        if (_logger == null) return;
        _logger.LogError($"[{prefix}] {message}");
    }

    public void LogError(Exception e)
    {
        if (_logger == null) return;
        _logger.LogError(e);
    }
}
