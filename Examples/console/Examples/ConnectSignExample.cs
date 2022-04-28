using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Models.Ethereum.Types;
using WalletConnectSharp.Desktop;

namespace WalletConnectSharp.Examples.Examples
{
    public class ConnectSignExample : IExample
    {
        public class Person
        {
            public string name;
            
            [EvmType("address")]
            public string wallet;

            public Person(string name, string wallet)
            {
                this.name = name;
                this.wallet = wallet;
            }
        }
        public class Mail
        {
            public Person from;
            public Person to;
            public string contents;

            public Mail(Person @from, Person to, string contents)
            {
                this.@from = @from;
                this.to = to;
                this.contents = contents;
            }
        }

        public async Task TestTypedSign(WalletConnectSession session)
        {
            Console.WriteLine("Sending EthSignTypedData");
            
            var domain = new EIP712Domain("Ether Name", "1", 1, "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC");
            
            var from = new Person("Cow", "0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826");
            var to = new Person("Bob", "0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB");
            var mail = new Mail(from, to, "Hello, Bob!");
            
            var result = await session.EthSignTypedData(session.Accounts[0], mail, domain);
            
            Console.WriteLine("Response: " + result);
        }
        
        public string Name
        {
            get
            {
                return "connect_sign_example";
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

            var client = new WalletConnect(clientMeta, "https://f.bridge.walletconnect.org");
            
            Console.WriteLine("Connect using the following URL");
            Console.WriteLine(client.URI);

            await client.ConnectSession();
            
            Console.WriteLine("The account " + client.Accounts[0] + " has connected!");

            var firstAccount = client.Accounts[0];

            /*
            Console.WriteLine("Sending sign request");

            await client.AddEthereumChain(new EthChainData()
            {
                blockExplorerUrls = new []{ "https://cchain.explorer.avax.network/" },
                chainId = "43114",
                chainName = "Avalanche Network",
                iconUrls = new []{ "https://pbs.twimg.com/profile_images/1414622406350495748/mw-nN8m0_400x400.jpg" },
                nativeCurrency = new NativeCurrency()
                {
                    decimals = 18,
                    name = "AVAX",
                    symbol = "AVAX"
                },
                rpcUrls = new []{"https://api.avax.network/ext/bc/C/rpc"}
            });

            var response = await client.EthPersonalSign(firstAccount, "This is a test!");
            
            Console.WriteLine("Got response: " + response);

            await TestTypedSign(client);
            */
            Thread.Sleep(1000);
            var td = new TransactionData()
            {
                chainId = 1,
                nonce = "0",
                gas = "10000",
                gasPrice = "1000000",
                from = client.Accounts[0],
                to = "0x5033d0D9b04Cca3E856e0b24887c366066C52E96",
                value = "1",
                data = "deposit()"
            };
            client.UseEthSignFallback = true;
            var sig = await client.EthSignTransaction(td);
            
            Console.WriteLine("Got sig: " + sig);

            Thread.Sleep(50000);

            await client.Disconnect();
            
            Console.WriteLine("All done");
        }
    }

    
}