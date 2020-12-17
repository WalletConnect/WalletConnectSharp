using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using WalletConnectSharp;
using WalletConnectSharp.Models;

namespace WalletConnectTest
{
    internal class Program
    {
        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger TokenAmount { get; set; }
        }
        
        [Function("balanceOf", "uint256")]
        public class ERC20Balance : FunctionMessage
        {
            [Parameter("address", "_owner", 1)]
            public virtual string Owner { get; set; }
        }
        
        public static async Task Main(string[] args)
        {
            var metadata = new ClientMeta()
            {
                Description = "This is a test of the Nethereum.WalletConnect feature",
                Icons = new[] {"https://app.warriders.com/favicon.ico"},
                Name = "WalletConnect Test",
                URL = "https://app.warriders.com"
            };
            
            var walletConnect = new WalletConnect(metadata);
            
            Console.WriteLine("Please connect your wallet: " + walletConnect.URI);

            var walletConnectData = await walletConnect.Connect();
            
            var web3 = new Web3(walletConnect.CreateProvider("b3b08fefe30e4579b62be25152d77044"));

            var balance = await web3.Eth.GetContractQueryHandler<ERC20Balance>().QueryAsync<BigInteger>(
                "0x6524B87960c2d573AE514fd4181777E7842435d4", new ERC20Balance()
                {
                    Owner = walletConnectData.accounts[0]
                });
            
            var fff = await web3.Eth.GetContractTransactionHandler<TransferFunction>().SendRequestAsync(
                "0x6524B87960c2d573AE514fd4181777E7842435d4", new TransferFunction()
                {
                    FromAddress = walletConnectData.accounts[0],
                    To = "0xc4ba9659442360ffe327aBf93E3d9aE0A838a8D2",
                    TokenAmount = balance / new BigInteger(2)
                });
            
            Console.WriteLine(balance);
            Console.WriteLine(fff);
        }
    }
}