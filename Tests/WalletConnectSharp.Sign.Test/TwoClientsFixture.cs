using System.Threading.Tasks;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Tests.Common;

namespace WalletConnectSharp.Sign.Test
{
    public abstract class TwoClientsFixture<SClient> where SClient : ISignClient
    {
        public SClient ClientA { get; protected set; }
        public SClient ClientB { get; protected set; }
        
        public SignClientOptions OptionsA { get; }
        public SignClientOptions OptionsB { get; }
        
        public TwoClientsFixture()
        {
            OptionsA = new SignClientOptions()
            {
                ProjectId = TestValues.TestProjectId,
                RelayUrl = TestValues.TestRelayUrl,
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
                RelayUrl = TestValues.TestRelayUrl,
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

        protected abstract void Init();

        public async Task WaitForClientsReady()
        {
            while (ClientA == null || ClientB == null)
                await Task.Delay(10);
        }
    }
}
