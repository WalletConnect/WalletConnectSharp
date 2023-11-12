using WalletConnectSharp.Core;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;
using WalletConnectSharp.Tests.Common;

namespace WalletConnectSharp.Sign.Test;

public class SignClientFixture : TwoClientsFixture<WalletConnectSignClient>
{
    public IKeyValueStorage StorageOverrideA;
    public IKeyValueStorage StorageOverrideB;

    public SignClientOptions OptionsA { get; protected set; }
    public SignClientOptions OptionsB { get;  protected set; }

    public SignClientFixture() : this(true) { }

    internal SignClientFixture(bool initNow) : base(initNow)
    {
    }

    public override async Task Init()
    {
        OptionsA = new SignClientOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            Metadata = new Metadata()
            {
                Description = "An example dapp to showcase WalletConnectSharpv2",
                Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                Name = $"WalletConnectSharpv2 Dapp Example",
                Url = "https://walletconnect.com"
            },
            // Omit if you want persistant storage
            Storage = StorageOverrideA ?? new InMemoryStorage()
        };
            
        OptionsB = new SignClientOptions()
        {
            ProjectId = TestValues.TestProjectId,
            RelayUrl = TestValues.TestRelayUrl,
            Metadata = new Metadata()
            {
                Description = "An example wallet to showcase WalletConnectSharpv2",
                Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                Name = $"WalletConnectSharpv2 Wallet Example",
                Url = "https://walletconnect.com"
            },
            // Omit if you want persistant storage
            Storage = StorageOverrideB ?? new InMemoryStorage()
        };
        
        ClientA = await WalletConnectSignClient.Init(OptionsA);
        ClientB = await WalletConnectSignClient.Init(OptionsB);
    }
    
    
}
