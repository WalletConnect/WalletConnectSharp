using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;

namespace WalletConnectSharp.Common.Model.Errors
{
    public class WalletConnectException : Exception
    {
        [JsonProperty("code")]
        public uint Code { get; private set; }
        
        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonIgnore]
        public ErrorType CodeType
        {
            get
            {
                return (ErrorType)Code;
            }
        }

        public WalletConnectException(string message, ErrorType type) : base(message)
        {
            Code = (uint) type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        public WalletConnectException(string message, Exception innerException, ErrorType type) : base(message, innerException)
        {
            Code = (uint) type;
            Type = Enum.GetName(typeof(ErrorType), type);
        }

        public static WalletConnectException FromType(ErrorType type, string message = null, Dictionary<string, object> @params = null, Exception innerException = null)
        {
            string errorMessage = SdkErrors.MessageFromType(type, message, @params);

            if (innerException != null)
                return new WalletConnectException(errorMessage, innerException, type);
            return new WalletConnectException(errorMessage, type);
        }

        public static WalletConnectException FromType(ErrorType type, object @params = null)
        {
            return FromType(type, null, @params.AsDictionary());
        }
    }
}