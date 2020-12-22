using System;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Network
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public NetworkMessage Message { get; private set; }
        
        public ITransport Source { get; private set; }

        public MessageReceivedEventArgs(NetworkMessage message, ITransport source)
        {
            Message = message;
            Source = source;
        }
    }
}