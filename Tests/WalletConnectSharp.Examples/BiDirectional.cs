using WalletConnectSharp.Core;
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
            var home = 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            var testAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
            
            var dappFilePath = Path.Combine(home, ".wc", "store_dapp_example.json");
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
                Storage = new FileSystemStorage(dappFilePath)
            };
            
            var dappConnectOptions = new ConnectOptions()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155", new ProposedNamespace()
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
                                "chainChanged", 
                                "accountsChanged",
                            }
                        }
                    }
                }
            };

            var dappClient = await WalletConnectSignClient.Init(dappOptions);
            var connectData = await dappClient.Connect(dappConnectOptions);

            var walletFilePath = Path.Combine(home, ".wc", "store_wallet_example.json");
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
                Storage = new FileSystemStorage(walletFilePath)
            };

            var walletClient = await WalletConnectSignClient.Init(walletOptions);

            var proposal = await walletClient.Pair(connectData.Uri);

            var approveData = await walletClient.Approve(proposal.ApproveProposal(testAddress));

            var sessionData = await connectData.Approval;
            await approveData.Acknowledged();

            Console.WriteLine("Done");
        }
    }
}
