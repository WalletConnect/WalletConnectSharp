using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models.Engine
{
    /// <summary>
    /// A class that represents parameters for rejecting a session proposal. Contains the id
    /// of the session proposal to reject and the <see cref="ErrorResponse"/> reason the session
    /// proposal was rejected
    /// </summary>
    public class RejectParams
    {
        /// <summary>
        /// The id of the session proposal to reject
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// The reason the session proposal was rejected, as an <see cref="ErrorResponse"/>
        /// </summary>
        [JsonProperty("reason")]
        public ErrorResponse Reason { get; set; }
    }
}
