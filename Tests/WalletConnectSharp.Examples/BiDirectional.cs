using System;
using System.Threading.Tasks;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Storage;

namespace WalletConnectSharp.Examples
{

    public class BiDirectional : IExample
    {
        public string Name
        {
            get { return "bi-directional"; }
        }

        public async Task Execute(string[] args)
        {
            var testAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";

            var dappOptions = new SignClientOptions()
            {
                ProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c",
                Metadata = new Metadata()
                {
                    Description = "An example dapp to showcase WalletConnectSharpv2",
                    Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                    Name = "WalletConnectSharpv2 Dapp Example",
                    Url = "https://walletconnect.com"
                },
                // Omit if you want persistant storage
                Storage = new InMemoryStorage()
            };

            var dappConnectOptions = new ConnectParams()
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
            };

            var dappClient = await WalletConnectSignClient.Init(dappOptions);
            var connectData = await dappClient.Connect(dappConnectOptions);

            var walletOptions = new SignClientOptions()
            {
                ProjectId = "39f3dc0a2c604ec9885799f9fc5feb7c",
                Metadata = new Metadata()
                {
                    Description = "An example wallet to showcase WalletConnectSharpv2",
                    Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                    Name = "WalletConnectSharpv2 Wallet Example",
                    Url = "https://walletconnect.com"
                },
                // Omit if you want persistant storage
                Storage = new InMemoryStorage()
            };

            var walletClient = await WalletConnectSignClient.Init(walletOptions);

            var pairing = await walletClient.Pair(new PairParams()
            {
                Uri = connectData.Uri
            });

            var proposal = await pairing.FetchProposal;

            var approveData = await walletClient.Approve(proposal.ApproveProposal(testAddress));

            var sessionData = await connectData.Approval;
            await approveData.Acknowledged();

            Console.WriteLine("Done");
        }
    }
}