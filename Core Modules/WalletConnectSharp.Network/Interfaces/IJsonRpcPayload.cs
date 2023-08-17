using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// An interface describing the fields every JSON RPC request/response/error must inlude
    /// </summary>
    public interface IJsonRpcPayload
    {
        /// <summary>
        /// The unique id for this JSON RPC payload
        /// </summary>
        long Id { get; }
        
        /// <summary>
        /// The version of this JSON RPC payload
        /// </summary>
        string JsonRPC { get; }
    }
}
