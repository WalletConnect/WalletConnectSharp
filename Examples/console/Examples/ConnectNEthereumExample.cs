using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Desktop;
using WalletConnectSharp.NEthereum;

namespace WalletConnectSharp.Examples.Examples
{
    public class ConnectNEthereumExample : IExample
    {
        public static readonly string PROJECT_ID = "";
        
        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger TokenAmount { get; set; }
        }
        
        public string Name
        {
            get
            {
                return "nethereum_example";
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

            var rpcEndpoint = "https://eth-mainnet.alchemyapi.io/v2/" + PROJECT_ID;
            
            Console.WriteLine("Connect using the following URL");
            Console.WriteLine(client.URI);

            await client.Connect();
            
            Console.WriteLine("The account " + client.Accounts[0] + " has connected!");

            Console.WriteLine("Using RPC endpoint " + rpcEndpoint + " as the fallback RPC endpoint");
            
            //We use an External Account so we can sign transactions
            var web3 = client.BuildWeb3(new Uri(rpcEndpoint)).AsExternalSigner();

            var firstAccount = client.Accounts[0];

            var secondAccount = "0x78F7911996e6803f26e180d21d78949f0fa386EA";

            Console.WriteLine("Sending test transactions from " + firstAccount + " to " + secondAccount);
            
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = secondAccount,
                TokenAmount = 100
            };
            var transactionReceipt = await transferHandler.SendRequestAndWaitForReceiptAsync(firstAccount, transfer);
            
            Console.WriteLine(transactionReceipt);


            await client.Disconnect();
        }
    }
}