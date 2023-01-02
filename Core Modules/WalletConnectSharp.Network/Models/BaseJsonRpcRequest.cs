namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A base class for creating custom JSON RPC request objects with a given parameter type T
    /// </summary>
    /// <typeparam name="T">The parameter type for this JSON RPC request</typeparam>
    public abstract class BaseJsonRpcRequest<T> : IRequestArguments<T>
    {
        public abstract string Method { get; }
        public T Params { get; protected set; }

        protected BaseJsonRpcRequest()
        {
        }

        protected BaseJsonRpcRequest(T @params)
        {
            Params = @params;
        }
    }
}