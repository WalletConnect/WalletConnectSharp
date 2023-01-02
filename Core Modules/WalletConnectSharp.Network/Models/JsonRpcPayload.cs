using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Represents a generic JSON RPC payload that may be a response/request/error, with properties to determine
    /// which it is
    /// </summary>
    public class JsonRpcPayload : IJsonRpcPayload
    {
        /// <summary>
        /// The JSON RPC id for this payload
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// The JSON RPC version for this payload
        /// </summary>
        public string JsonRPC { get; set; }
        
        [JsonExtensionData]
#pragma warning disable CS0649
        private IDictionary<string, JToken> _extraStuff;
#pragma warning restore CS0649

        /// <summary>
        /// Get the method for this payload, if this payload is a request.
        /// If this payload is not a request, then an error is thrown
        /// </summary>
        public string Method
        {
            get
            {
                if (!IsRequest)
                    throw new ArgumentException("The given payload is not a request, and thus has no Method");

                return _extraStuff["method"].ToObject<string>();
            }
        }
        
        /// <summary>
        /// Whether this payload represents a request
        /// </summary>
        public bool IsRequest
        {
            get
            {
                return _extraStuff.ContainsKey("method");
            }
        }

        /// <summary>
        /// Whether this payload represents a response
        /// </summary>
        public bool IsResponse
        {
            get
            {
                return _extraStuff.ContainsKey("result") || IsError;
            }
        }

        /// <summary>
        /// Whether this payload represents an error
        /// </summary>
        public bool IsError
        {
            get
            {
                return _extraStuff.ContainsKey("error");
            }
        }

        public JsonRpcPayload()
        {
        }

        public JsonRpcPayload(long id, string jsonRpc)
        {
            Id = id;
            JsonRPC = jsonRpc;
        }
    }
}