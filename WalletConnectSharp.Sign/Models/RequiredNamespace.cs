using System;
using System.Linq;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models
{
    public class RequiredNamespace : BaseRequiredNamespace<RequiredNamespace>
    {
        [JsonProperty("extension", NullValueHandling=NullValueHandling.Ignore)]
        public BaseRequiredNamespace<RequiredNamespace>[] Extension { get; set; }

        public RequiredNamespace() : base()
        {
            Extension = Array.Empty<BaseRequiredNamespace<RequiredNamespace>>();
        }

        public RequiredNamespace WithExtension(BaseRequiredNamespace<RequiredNamespace> requiredNamespace)
        {
            Extension = Extension.Append(requiredNamespace).ToArray();
            return this;
        }
    }
}