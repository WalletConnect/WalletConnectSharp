namespace WalletConnectSharp.Tests.Common;

public abstract class TwoClientsFixture<TClient>
{
    public TClient ClientA { get; protected set; }
    public TClient ClientB { get; protected set; }
    

    public TwoClientsFixture()
    {
        Init();
    }

    protected abstract void Init();

    public async Task WaitForClientsReady()
    {
        while (ClientA == null || ClientB == null)
            await Task.Delay(10);
    }
}
