namespace WalletConnectSharp.Core.Interfaces
{
    public interface ISubscriberMap
    {
        public string[] Topics { get; }

        public void Set(string topic, string id);

        public string[] Get(string topic);

        public bool Exists(string topic, string id);

        public void Delete(string topic, string id = null);

        public void Clear();
    }
}