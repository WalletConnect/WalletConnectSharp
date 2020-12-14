using System;
using System.Threading.Tasks;
using WalletConnectSharp.Events;
using WalletConnectSharp.Models;

namespace WalletConnectSharp.Network
{
    public interface ITransport : IDisposable
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        Task Open(string bridgeURL);

        Task Close();

        Task SendMessage(NetworkMessage message);

        Task Subscribe(string topic);

        Task Subscribe<T>(string topic, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse;

        Task Subscribe<T>(string topic, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest;
        
        void ListenFor<T>(string eventId, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse;
        
        void ListenFor<T>(string eventId, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest;
        
        void Trigger(string topic, string json);
    }
}