using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class BaseRequiredNamespace<T> where T : BaseRequiredNamespace<T>
    {
        [JsonProperty("chains")]
        public string[] Chains { get; set; }
        
        [JsonProperty("methods")]
        public string[] Methods { get; set; }
        
        [JsonProperty("events")]
        public string[] Events { get; set; }

        public BaseRequiredNamespace()
        {
            Chains = Array.Empty<string>();
            Methods = Array.Empty<string>();
            Events = Array.Empty<string>();
        }

        public T WithChain(string chain)
        {
            Chains = Chains.Append(chain).ToArray();
            return this as T;
        }

        public T WithMethod(string method)
        {
            Methods = Methods.Append(method).ToArray();
            return this as T;
        }
        
        public T WithEvent(string @event)
        {
            Events = Events.Append(@event).ToArray();
            return this as T;
        }
    }
}