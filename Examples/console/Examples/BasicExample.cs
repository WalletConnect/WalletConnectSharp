using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Desktop;

namespace WalletConnectSharp.Examples.Examples;

public class BasicExample : IExample
{
    public string Name
    {
        get
        {
            return "basic_example";
        }
    }

    public async Task Execute(string[] args)
    {
        var clientMeta = new ClientMeta()
        {
            Name = "WalletConnectSharp",
            Description = "An example that showcases how to use the WalletConnectSharp library",
            Icons = new[] { "https://app.warriders.com/favicon.ico" },
            URL = "https://app.warriders.com/"
        };

        var client = new WalletConnect(clientMeta);

        Console.WriteLine("Connect using the following URL");
        Console.WriteLine(client.URI);

        await client.Connect();

        while (true)
        {
            Console.WriteLine($"Address: {client.Accounts[0]}");
            Console.WriteLine($"Chain ID: {client.ChainId}");
            await Task.Delay(3000);
        }
    }
}
