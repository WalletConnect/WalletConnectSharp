using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WalletConnectSharp.Examples.Examples;

namespace WalletConnectSharp.Examples
{
    class Program
    {
        private static readonly IExample[] Examples = new IExample[]
        {
            new NEthereumSendTransactionExample()
        };

        static void ShowHelp()
        {
            Console.WriteLine("Please specify which example to run");
            foreach (var e in Examples)
            {
                Console.WriteLine("    - " + e.Name);
            }
        }

        static async Task Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    ShowHelp();
            //    return;
            //}

            string name = "nethereum_send_tx_example";
            string[] exampleArgs = args.Skip(1).ToArray();

            var example = Examples.FirstOrDefault(e => e.Name.ToLower() == name);

            if (example == null)
            {
                ShowHelp();
                return;
            }
            
            await example.Execute(exampleArgs);
        }
    }
}