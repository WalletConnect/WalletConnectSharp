using System.Threading.Tasks;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Tests.Common;

namespace WalletConnectSharp.Sign.Test
{
    public class TwoClientsFixture
    {
        public WalletConnectSignClient ClientA { get; private set; }
        public WalletConnectSignClient ClientB { get; private set; }
        
        public SignClientOptions OptionsA { get; }
        public SignClientOptions OptionsB { get; }
        
        public TwoClientsFixture()
        {
            OptionsA = new SignClientOptions()
            {
                ProjectId = TestValues.TestProjectId,
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
            
            OptionsB = new SignClientOptions()
            {
                ProjectId = TestValues.TestProjectId,
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

            Init();
        }

        private async void Init()
        {
            ClientA = await WalletConnectSignClient.Init(OptionsA);
            ClientB = await WalletConnectSignClient.Init(OptionsB);
        }

        public async Task WaitForClientsReady()
        {
            while (ClientA == null || ClientB == null)
                await Task.Delay(10);
        }
    }
}