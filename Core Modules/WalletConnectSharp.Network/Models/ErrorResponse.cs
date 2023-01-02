using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Indicates an error
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error code of this error
        /// </summary>
        [JsonProperty("code")]
        public long Code;

        /// <summary>
        /// The message for this error
        /// </summary>
        [JsonProperty("message")]
        public string Message;

        /// <summary>
        /// Any extra data for this error
        /// </summary>
        [JsonProperty("data")]
        public string Data;

        public static ErrorResponse FromErrorType(ErrorType type, object @params = null, string extraData = null)
        {
            string message = SdkErrors.MessageFromType(type, @params);

            return new ErrorResponse()
            {
                Code = (long) type,
                Message = message,
                Data = extraData
            };
        }

        public static ErrorResponse FromException(WalletConnectException walletConnectException)
        {
            return new ErrorResponse()
            {
                Code = walletConnectException.Code,
                Message = walletConnectException.Message,
                Data = walletConnectException.ToString()
            };
        }

        public WalletConnectException ToException()
        {
            return WalletConnectException.FromType((ErrorType)Code, Message);
        }
    }
}