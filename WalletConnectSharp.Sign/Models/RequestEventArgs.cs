using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models
{
    public class RequestEventArgs<T, TR>
    {
        public string Topic { get; }
        
        public JsonRpcRequest<T> Request { get; }
        
        public TR Response { get; set; }
        
        public bool Cancelled { get; set; }
        
        public ErrorResponse Error { get; set; }

        public RequestEventArgs(string topic, JsonRpcRequest<T> request)
        {
            Topic = topic;
            Request = request;
        }
    }
}