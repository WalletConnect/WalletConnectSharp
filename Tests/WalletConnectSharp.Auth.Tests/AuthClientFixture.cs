using WalletConnectSharp.Auth.Interfaces;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Tests.Common;

namespace WalletConnectSharp.Auth.Tests;

public class AuthClientFixture : TwoClientsFixture<IAuthClient>
{
    public AuthOptions OptionsA { get; protected set; }
    
    public AuthOptions OptionsB { get; protected set; }

    protected override async void Init()
    {
        OptionsA = new AuthOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            Metadata = new AuthMetadata()
            {
                Description = "An example dapp to showcase WalletConnectSharpv2",
                Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                Name = "WalletConnectSharpv2 Dapp Example",
                Url = "https://walletconnect.com"
            },
            // Omit if you want persistant storage
            Storage = new InMemoryStorage(),
        };

        OptionsB = new AuthOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            Metadata = new AuthMetadata()
            {
                Description = "An example wallet to showcase WalletConnectSharpv2",
                Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                Name = "WalletConnectSharpv2 Wallet Example",
                Url = "https://walletconnect.com"
            },
            // Omit if you want persistant storage
            Storage = new InMemoryStorage()
        };

        ClientA = await WalletConnectAuthClient.Init(OptionsA);
        ClientB = await WalletConnectAuthClient.Init(OptionsB);
    }
}
