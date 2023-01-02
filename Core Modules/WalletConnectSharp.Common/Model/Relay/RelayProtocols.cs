using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WalletConnectSharp.Common.Model.Relay
{
    public abstract class RelayProtocols
    {
        public static readonly string Default = "irn";
        public static RelayProtocols Waku = new WakuRelayProtocol();
        public static RelayProtocols Irn = new IrnRelayProtocol();
        public static RelayProtocols Iridium = new IridiumRelayProtocol();
        
        private static Dictionary<string, RelayProtocols> _protocols = new Dictionary<string, RelayProtocols>()
        {
            { "waku", Waku },
            { "irn", Irn },
            { "iridium", Iridium },
        };

        public static IReadOnlyDictionary<string, RelayProtocols> Protocols => _protocols;

        public static RelayProtocols GetRelayProtocol(string protocol)
        {
            if (Protocols.ContainsKey(protocol))
                return Protocols[protocol];

            throw new ArgumentException("Relay Protocol not supported: " + protocol);
        }
        
        [JsonProperty("publish")]
        public abstract string Publish { get; }
        
        [JsonProperty("subscribe")]
        public abstract string Subscribe { get; }
        
        [JsonProperty("subscription")]
        public abstract string Subscription { get; }
        
        [JsonProperty("unsubscribe")]
        public abstract string Unsubscribe { get; }

        public class WakuRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "waku_publish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "waku_subscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "waku_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "waku_unsubscribe";
                }
            }
        }

        public class IrnRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "irn_publish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "irn_subscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "irn_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "irn_unsubscribe";
                }
            }
        }

        public class IridiumRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "iridium_publish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "iridium_subscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "iridium_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "iridium_unsubscribe";
                }
            }
        }

        public static RelayProtocols DefaultProtocol => GetRelayProtocol(Default);
    }
}