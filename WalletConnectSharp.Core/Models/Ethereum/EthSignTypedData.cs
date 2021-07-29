using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Ethereum.Types;

namespace WalletConnectSharp.Core.Models.Ethereum
{
    public class EthSignTypedData<T> : JsonRpcRequest
    {
        [JsonProperty("params")] 
        private object[] _parameters;
        
        public EthSignTypedData(string address, T data, EIP712Domain domain)
        {
            this.Method = "eth_signTypedData";
            this._parameters = new object[] {address, new EvmTypedData<T>(data, domain)};
        }
    }
}