using System;
using System.Threading.Tasks;
using WalletConnectSharp;
using WalletConnectSharp.Events;
using WalletConnectSharp.Models;

namespace WalletConnectSharpTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var clientMeta = new ClientMeta()
            {
                Description = "This is a test of the WalletConnectSharp Library!",
                Icons = new[] {"https://app.warriders.com/favicon.ico"},
                Name = "WalletConnectSharp Test",
                URL = "https://app.warriders.com"
            };
            
            WalletConnect walletConnect = new WalletConnect(clientMeta);
            
            Console.WriteLine(walletConnect.URI);

            var result = await walletConnect.Connect();
            
            Console.WriteLine(result.result.peerMeta);
        }
    }
}