using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// The data for an Iridium JWT payload 
    /// </summary>
    [Serializable]
    public class IridiumJWTPayload
    {
        /// <summary>
        /// The iss value
        /// </summary>
        [JsonProperty("iss")] public string Iss;

        /// <summary>
        /// The sub value
        /// </summary>
        [JsonProperty("sub")] public string Sub;

        /// <summary>
        /// The aud value
        /// </summary>
        [JsonProperty("aud")] public string Aud;

        /// <summary>
        /// The iat value
        /// </summary>
        [JsonProperty("iat")] public long Iat;

        /// <summary>
        /// The exp value
        /// </summary>
        [JsonProperty("exp")] public long Exp;
    }
}
