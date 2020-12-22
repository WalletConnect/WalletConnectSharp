namespace WalletConnectSharp.Events
{
    public class GenericEventArgs<T>
    {
        public T Response { get; private set; }

        public GenericEventArgs(T response)
        {
            Response = response;
        }
    }
}