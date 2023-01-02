using System;
using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Expirer
{
    public class ExpirerTarget
    {
        [JsonProperty("id")]
        public long? Id { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }

        public ExpirerTarget(string target)
        {
            var values = target.Split(":");
            var type = values[0];
            var value = values[1];

            switch (type)
            {
                case "topic":
                    Topic = value;
                    break;
                case "id":
                {
                    long id;
                    var success = long.TryParse(value, out id);

                    if (!success)
                        throw new Exception($"Cannot parse id {value}");

                    Id = id;
                    break;
                }
                default:
                    throw new Exception($"Invalid target, expected id:number or topic:string, got {type}:{value}");
            }
        }
    }
}