using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Desktop;
using WalletConnectSharp.NEthereum;

namespace WalletConnectSharp.Examples.Examples
{
    public class NEthereumSendTransactionExample : IExample
    {
        public static readonly string PROJECT_ID = "";
        
        [Function("deposit", "bool")]
        public class WEthDepositFunction : FunctionMessage
        {
            [Parameter("uint256", "payableAmount")]
            public BigInteger EthAmount { get; set; }
        }
        
        [Function("deposit", "bool")]
        public class DepositFunction : FunctionMessage
        {
        }
        
        public string Name
        {
            get
            {
                return "nethereum_send_tx_example";
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

            var rpcEndpoint = "https://cloudflare-eth.com" + PROJECT_ID;
            
            Console.WriteLine("Connect using the following URL");
            Console.WriteLine(client.URI);

            await client.Connect();
            
            Console.WriteLine("The account " + client.Accounts[0] + " has connected!");

            Console.WriteLine("Using RPC endpoint " + rpcEndpoint + " as the fallback RPC endpoint");
            
            //We use an External Account so we can sign transactions
            var web3 = client.BuildWeb3(new Uri(rpcEndpoint)).AsWalletAccount(true);

            var firstAccount = client.Accounts[0];

            var secondAccount = "0x78F7911996e6803f26e180d21d78949f0fa386EA";

            Console.WriteLine("Sending test transactions from " + firstAccount + " to " + secondAccount);
            
            var transferHandler = web3.Eth.GetContractTransactionHandler<WEthDepositFunction>();

            var transfer = new WEthDepositFunction()
            {
                EthAmount = 1
            };
            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(firstAccount, transfer);
            
            Console.WriteLine(transactionReceipt);
            
            await client.Disconnect();
        }
    }
}