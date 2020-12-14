using System;
using System.IO;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.WalletConnect.Client;
using Nethereum.WalletConnect.Models;
using Nethereum.WalletConnect.Network;
using Nethereum.WalletConnect.Utils;
using Nethereum.Web3;
using Newtonsoft.Json;

namespace NethereumTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
/*            AESCipher test = new AESCipher();

            var hexKey = "3d5fbb4e9c069699a538d67c30aab4092f5b5a8076c2df6381c1fe92358fe55b";

            byte[] keyArray = hexKey.FromHex();

            var json = File.ReadAllText("../../test.json");
            
            var networkMessage = JsonConvert.DeserializeObject<NetworkMessage>(json);

            var encryptedMessage = JsonConvert.DeserializeObject<EncryptedPayload>(networkMessage.Payload);

            var decrypted = await test.DecryptWithKey(keyArray, encryptedMessage);
            
            Console.WriteLine(decrypted);*/

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

            var web3 = new Web3(walletConnect.CreateProvider());

            var balance = await web3.TransactionManager.SendTransactionAsync(walletConnectData.accounts[0],
                "0xc4ba9659442360ffe327aBf93E3d9aE0A838a8D2", new HexBigInteger("10"));
            
            Console.WriteLine(balance);
        }
    }
}