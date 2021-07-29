using System;
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

            var json = JsonConvert.SerializeObject(new EthSignTypedData<Mail>(session.Accounts[0], mail, domain));
            
            Console.WriteLine(json);
            
            
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

            var client = new WalletConnect(clientMeta);
            
            Console.WriteLine("Connect using the following URL");
            Console.WriteLine(client.URI);

            await client.Connect();
            
            Console.WriteLine("The account " + client.Accounts[0] + " has connected!");

            var firstAccount = client.Accounts[0];

            Console.WriteLine("Sending sign request");

            var response = await client.EthPersonalSign(firstAccount, "This is a test!");
            
            Console.WriteLine("Got response: " + response);

            await TestTypedSign(client);

            await client.Disconnect();
            
            Console.WriteLine("All done");
        }
    }

    
}