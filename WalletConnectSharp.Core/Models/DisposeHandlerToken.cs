namespace WalletConnectSharp.Core.Models;

public class DisposeHandlerToken : IDisposable
{
    private readonly Action _onDispose;
    
    public DisposeHandlerToken(Action onDispose)
    {
        if (onDispose == null)
            throw new ArgumentException("onDispose must be non-null");
        this._onDispose = onDispose;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed) return;
            
        if (disposing)
        {
            this._onDispose();
        }

        Disposed = true;
    }

    protected bool Disposed;
}
