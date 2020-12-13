using System.IO;
using Newtonsoft.Json;

namespace WalletConnectSharp.Models
{
    public class JsonRpcResponse : IEventSource
    {
        [JsonProperty]
        private int id;
        
        [JsonProperty]
        private string jsonrpc = "2.0";

        [JsonProperty]
        private JsonRpcError error;

        public JsonRpcError Error
        {
            get { return error; }
        }

        public bool IsError
        {
            get { return error != null; }
        }

        public int ID
        {
            get { return id; }
        }

        public string JsonRPC
        {
            get { return jsonrpc; }
        }

        public class JsonRpcError
        {
            [JsonProperty]
            private int? code;
            
            [JsonProperty]
            private string message;

            public int? Code
            {
                get { return code; }
            }

            public string Message
            {
                get { return message; }
            }
        }

        public string Event
        {
            get { return "response:" + ID; }
        }
    }
}