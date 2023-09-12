namespace WalletConnectSharp.Common.Utils
{
    public class ListComparer<T> : IEqualityComparer<List<T>>
    {
        private IEqualityComparer<T> valueComparer;
        public ListComparer(IEqualityComparer<T> valueComparer = null)
        {
            this.valueComparer = valueComparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(List<T> x, List<T> y)
        {
            return x.SetEquals(y, valueComparer);
        }

        public int GetHashCode(List<T> obj)
        {
            throw new NotImplementedException();
        }
    }
}
