using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Desktop;

namespace WalletConnectSharp.Examples.Examples;

public class BiDirectionalExample : IExample
{
    public string Name
    {
        get
        {
            return "bidirectional_example";
        }
    }

    public async Task Execute(string[] args)
    {
        var appMetadata = new ClientMeta()
        {
            Name = "WalletConnectSharp",
            Description = "An example that showcases how to use the WalletConnectSharp library",
            Icons = new[] { "https://walletconnect.com/favicon.ico" },
            URL = "https://walletconnect.com/"
        };

        var walletData = new StaticWalletData()
        {
            Accounts = new[] {"0x4EeABa74D7f51fe3202D7963EFf61D2e7e166cBa"},
            ChainId = 1,
            ClientMeta = new ClientMeta()
            {
                Name = "WalletConnectSharp Wallet",
                Description = "An example that showcases how to use WalletConnectSharp as a wallet",
                Icons = new[] {"https://walletconnect.com/favicon.ico"},
                URL = "https://walletconnect.com/"
            },
            NetworkId = 1
        };

        var client = new WalletConnect(appMetadata);

        Console.WriteLine("Connect using the following URL");
        Console.WriteLine(client.URI);

        var connectTask = client.Connect();

        while (!client.Connecting)
        {
            await Task.Delay(100);
        }

        await client.WaitForUserPromptReady();

        var wallet = new WalletConnectProvider(client.URI, walletData);

        await wallet.Connect();
        
        // We can now either accept or reject the session
        await wallet.AcceptRequest();

        await connectTask;

        while (true)
        {
            Console.WriteLine($"Address: {client.Accounts[0]}");
            Console.WriteLine($"Chain ID: {client.ChainId}");
            await Task.Delay(3000);
        }
    }
}
