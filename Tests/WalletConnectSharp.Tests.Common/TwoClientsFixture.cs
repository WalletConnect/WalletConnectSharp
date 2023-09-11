namespace WalletConnectSharp.Tests.Common;

public abstract class TwoClientsFixture<TClient>
{
    public TClient ClientA { get; protected set; }
    public TClient ClientB { get; protected set; }
    

    public TwoClientsFixture(bool initNow = true)
    {
        if (initNow)
            Init();
    }

    public abstract Task Init();

    public async Task WaitForClientsReady()
    {
        while (ClientA == null || ClientB == null)
            await Task.Delay(10);
    }
}
