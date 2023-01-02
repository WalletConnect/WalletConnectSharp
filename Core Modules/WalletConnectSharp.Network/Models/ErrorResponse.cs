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

        /// <summary>
        /// Create an ErrorResponse with a given ErrorType and (optional) parameters
        /// </summary>
        /// <param name="type">The error type of the ErrorResponse to create</param>
        /// <param name="params">Extra parameters for the error message</param>
        /// <param name="extraData">Extra data that is stored in the Data field of the newly created ErrorResponse</param>
        /// <returns>A new ErrorResponse</returns>
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

        /// <summary>
        /// Create an ErrorResponse from a WalletConnectException
        /// </summary>
        /// <param name="walletConnectException">The exception to grab error values from</param>
        /// <returns>A new ErrorResponse object using values from the given exception</returns>
        public static ErrorResponse FromException(WalletConnectException walletConnectException)
        {
            return new ErrorResponse()
            {
                Code = walletConnectException.Code,
                Message = walletConnectException.Message,
                Data = walletConnectException.ToString()
            };
        }

        /// <summary>
        /// Convert this ErrorResponse to a WalletConnectException
        /// </summary>
        /// <returns>A new WalletConnectException using values from this ErrorResponse</returns>
        public WalletConnectException ToException()
        {
            return WalletConnectException.FromType((ErrorType)Code, Message);
        }
    }
}
