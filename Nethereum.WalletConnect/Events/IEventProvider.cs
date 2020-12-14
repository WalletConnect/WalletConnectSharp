namespace Nethereum.WalletConnect.Events
{
    public interface IEventProvider
    {
        void PropagateEvent(string responseJson);
    }
}