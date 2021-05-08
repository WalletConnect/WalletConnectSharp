using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using QRCoder;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.Examples.Examples
{
    public class ConnectExample : IExample
    {
        public string Name
        {
            get
            {
                return "connect_example";
            }
        }

        public async Task Execute(string[] args)
        {
            var clientMeta = new ClientMeta()
            {
                Name = "WalletConnectSharp",
                Description = "An example that showcases how to use the WalletConnectSharp library",
                Icons = new []{ "https://app.warriders.com/favicon.ico" },
                URL = "https://app.warriders.com/"
            };

            var client = new WalletConnect(clientMeta);
            
            Console.WriteLine("Connect using the following URL");
            Console.WriteLine(client.URI);

            await client.Connect();
            
            Console.WriteLine("The account " + client.Accounts[0] + " has connected!");

            Console.WriteLine("Using RPC endpoint " + args[0] + " as the fallback RPC endpoint");
            var web3 = new Web3(client.CreateProvider(new Uri(args[0])));

            var firstAccount = client.Accounts[0];

            var secondAccount = args[1];
            
            Console.WriteLine("Sending test transactions from " + firstAccount + " to " + secondAccount);
            
            try
            {
                await web3.Eth.TransactionManager.SendTransactionAsync(firstAccount, secondAccount,
                    new BigInteger(50).ToHexBigInteger());
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                await web3.Eth.TransactionManager.SendTransactionAsync(firstAccount, secondAccount,
                    new BigInteger(50).ToHexBigInteger());
            } 
            catch(Exception e)
            {
                Console.WriteLine(e);
            }


            await client.Disconnect();
        }
    }
}