using System;
using System.Linq;

namespace WalletConnectSharp.Core.Network
{
    public static class DefaultBridge
    {
        public static string MainBridge = "https://bridge.walletconnect.org";
        
        private static string[] bridges = new[]
        {
            "https://a.bridge.walletconnect.org",
            "https://b.bridge.walletconnect.org",
            "https://c.bridge.walletconnect.org",
            "https://d.bridge.walletconnect.org",
            "https://e.bridge.walletconnect.org",
            "https://f.bridge.walletconnect.org",
            "https://g.bridge.walletconnect.org",
            "https://h.bridge.walletconnect.org",
            "https://i.bridge.walletconnect.org",
            "https://j.bridge.walletconnect.org",
            "https://k.bridge.walletconnect.org",
            "https://l.bridge.walletconnect.org",
        };

        private static string[] _bridgeCache = null;

        public static string[] AlternateBridges
        {
            get
            {
                return bridges;
            }
        }

        public static string[] AllBridges
        {
            get
            {
                return _bridgeCache ??
                       (_bridgeCache = Enumerable.Empty<string>().Append(MainBridge).Concat(bridges).ToArray());
            }
        }

        public static string ChooseRandomBridge(string[] possibleBridges = null)
        {
            if (possibleBridges == null)
            {
                possibleBridges = AllBridges;
            }

            var random = new Random();
            return possibleBridges[random.Next(possibleBridges.Length)];
        }
    }
}