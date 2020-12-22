namespace WalletConnectSharp.Events
{
    public interface IEventProvider
    {
        void PropagateEvent(string responseJson);
    }
}