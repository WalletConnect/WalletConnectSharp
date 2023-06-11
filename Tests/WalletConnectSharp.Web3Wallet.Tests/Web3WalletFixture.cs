using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Tests.Common;
using WalletConnectSharp.Web3Wallet.Interfaces;
using WalletConnectSharp.Web3Wallet;

namespace WalletConnectSharp.Auth.Tests;

public class Web3WalletFixture : TwoClientsFixture<IWeb3Wallet>
{
    public CoreOptions OptionsA { get; protected set; }
    
    public CoreOptions OptionsB { get; protected set; }
    
    public AuthMetadata MetadataA { get; protected set; }
    
    public AuthMetadata MetadataB { get; protected set; }
    
    public ICore CoreA { get; protected set; }
    
    public ICore CoreB { get; protected set; }

    protected override async void Init()
    {
        MetadataA = new AuthMetadata()
        {
            Description = "An example dapp to showcase WalletConnectSharpv2",
            Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
            Name = "WalletConnectSharpv2 Dapp Example",
            Url = "https://walletconnect.com"
        };
        
        OptionsA = new CoreOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            // Omit if you want persistant storage
            Storage = new InMemoryStorage(),
        };

        OptionsB = new CoreOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            // Omit if you want persistant storage
            Storage = new InMemoryStorage()
        };

        MetadataB = new AuthMetadata()
        {
            Description = "An example wallet to showcase WalletConnectSharpv2",
            Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
            Name = "WalletConnectSharpv2 Wallet Example",
            Url = "https://walletconnect.com"
        };

        CoreA = new Core.Core(OptionsA);

        ClientA = await Web3WalletClient.Init(OptionsA);
        ClientB = await Web3WalletClient.Init(OptionsB);
    }
}
