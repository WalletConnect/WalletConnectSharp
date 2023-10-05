namespace WalletConnectSharp.Core.Models.Heartbeat
{
    /// <summary>
    /// A class containing all events the Heartbeat module emits
    /// </summary>
    [Obsolete("These events have been replaced by C# events")]
    public static class HeartbeatEvents
    {
        /// <summary>
        /// The Pulse event id
        /// </summary>
        public static readonly string Pulse = "heartbeat_pulse";
    }
}
