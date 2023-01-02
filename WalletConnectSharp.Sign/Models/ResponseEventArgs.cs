using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models
{
    public class ResponseEventArgs<TR>
    {
        public string Topic { get; }
        
        public JsonRpcResponse<TR> Response { get; }

        public ResponseEventArgs(JsonRpcResponse<TR> response, string topic)
        {
            Response = response;
            Topic = topic;
        }
    }
}