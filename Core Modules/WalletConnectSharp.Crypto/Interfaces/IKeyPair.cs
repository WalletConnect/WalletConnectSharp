namespace WalletConnectSharp.Crypto.Interfaces
{
    public interface IKeyPair
    {
        string PrivateKey { get; }
        
        string PublicKey { get; }
    }
}