using System;
using System.Threading.Tasks;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Network
{
    public interface ITransport : IDisposable
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        Task Open(string bridgeURL);

        Task Close();

        void SendMessage(NetworkMessage message);
    }
}