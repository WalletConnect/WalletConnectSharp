using System;
using System.Threading.Tasks;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectSharp.Examples
{
    public class SimpleExample : IExample
    {
        public string Name
        {
            get { return "simple_example"; }
        }

        public async Task Execute(string[] args)
        {
            var options = new SignClientOptions()
            {
                ProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c",
                Metadata = new Metadata()
                {
                    Description = "An example project to showcase WalletConnectSharpv2",
                    Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                    Name = "WalletConnectSharpv2 Example",
                    Url = "https://walletconnect.com"
                }
            };

            var client = await WalletConnectSignClient.Init(options);

            var connectData = await client.Connect(new ConnectParams()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155", new RequiredNamespace()
                        {
                            Methods = new[]
                            {
                                "eth_sendTransaction",
                                "eth_signTransaction",
                                "eth_sign",
                                "personal_sign",
                                "eth_signTypedData",
                            },
                            Chains = new[]
                            {
                                "eip155:1"
                            },
                            Events = new[]
                            {
                                "chainChanged", "accountsChanged"
                            }
                        }
                    }
                }
            });

            Console.WriteLine(connectData.Uri);

            await connectData.Approval;

            Console.WriteLine("Connected");

            while (true)
            {
                await Task.Delay(2000);
            }
        }
    }
}