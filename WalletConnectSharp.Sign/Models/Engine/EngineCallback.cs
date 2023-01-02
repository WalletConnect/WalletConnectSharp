namespace WalletConnectSharp.Sign.Models.Engine
{
    public class EngineCallback<T>
    {
        public string Topic { get; set; }
        
        public T Payload { get; set; }
    }
}