using System.Numerics;
using System.Diagnostics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum.Types;
using WalletConnectSharp.Desktop;
using WalletConnectSharp.NEthereum;
using Nethereum.ABI.EIP712;

namespace WalletConnectSharp.Examples.Examples;

[Struct("Message")]
public class Message
{
    [EvmType("string", "stringValue", 1)]
    [JsonProperty("stringValue", Order = 1)]
    [Parameter("string", "stringValue", 1)]
    public string StringValue { get; set; }

    [EvmType("address", "addressValue", 2)]
    [JsonProperty("addressValue", Order = 2)]
    [Parameter("address", "addressValue", 2)]
    public string AddressValue { get; set; }

    [EvmType("uint8", "uint8Value", 3)]
    [JsonProperty("uint8Value", Order = 3)]
    [Parameter("uint8", "uint8Value", 3)]
    public ushort Uint8Value { get; set; }

    [EvmType("uint64", "uint64Value", 4)]
    [JsonProperty("uint64Value", Order = 4)]
    [Parameter("uint64", "uint64Value", 4)]
    public ulong Uint64Value { get; set; }

    [EvmType("uint256", "uint256Value", 5)]
    [JsonProperty("uint256Value", Order = 5)]
    [Parameter("uint256", "uint256Value", 5)]
    public ulong Uint256Value { get; set; }
}

/// <summary>
/// Example demonstrating different methods to sign typed data.
/// </summary>
public class SignTypedDataExample : IExample
{

    public string Name => "sign_typed_data_example";

    public const string VerifyingContractName = "Some Contract Name";
    public const string VerifyingContractVersion = "1.0";
    public static BigInteger VerifyingContractChainId => new(1);
    public const string VerifyingContractAddress = "0x0000000000000000000000000000000000000000";

    public static EIP712Domain WalletConnectDomain => new()
    {
        Name = VerifyingContractName,
        Version = VerifyingContractVersion,
        ChainId = VerifyingContractChainId,
        VerifyingContract = VerifyingContractAddress
    };

    public static Domain NethereumDomain => new()
    {
        Name = VerifyingContractName,
        Version = VerifyingContractVersion,
        ChainId = VerifyingContractChainId,
        VerifyingContract = VerifyingContractAddress
    };

    public TypedData<Domain> NethereumSchema => new()
    {
        Domain = NethereumDomain,
        Types = MemberDescriptionFactory.GetTypesMemberDescription(
                typeof(Domain),
                typeof(Message)
            ),
        PrimaryType = nameof(Message),
    };

    private static Eip712TypedDataSigner _signer => new();


    public async Task Execute(string[] args)
    {
        // create client metadata
        var clientMeta = new ClientMeta()
        {
            Name = "WalletConnectSharp",
            Description = "An example that showcases how to use the WalletConnectSharp library",
            Icons = new[] { "https://walletconnect.com/favicon.ico" },
            URL = "https://walletconnect.com/"
        };

        // create a client
        var client = new WalletConnect(clientMeta);

        Console.WriteLine("Connect using the following URL");
        Console.WriteLine(client.URI);

        await client.Connect();

        var account = Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(client.Accounts[0]);

        Console.WriteLine("The account " + account + " has connected!");

        // create a message to sign
        var message = new Message
        {
            StringValue = "Some String",
            AddressValue = account,
            Uint8Value = 123,
            Uint64Value = 12345,
            Uint256Value = 123456,
        };

        try
        {
            await Sign_With_WalletConnect_Serialization(client, account, message);
            await Sign_With_Nethereum_Serialization(client, account, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            await client.Disconnect();
        }

        Console.WriteLine($"{Name}: COMPLETE!");
    }

    private async Task Sign_With_WalletConnect_Serialization(WalletConnect client, string account, Message message)
    {
        Console.WriteLine($"Requesting Signature from {account}");

        var signature = await client.EthSignTypedData(account, message, WalletConnectDomain);

        Console.WriteLine($"WalletConnect signature: {signature}");

        var recovered = _signer.RecoverFromSignatureV4(
            message, NethereumSchema, signature);

        Console.WriteLine($"WalletConnect recovered: {recovered}");

        // assert the recovery matches the account
        Debug.Assert(recovered == account);
    }

    private async Task Sign_With_Nethereum_Serialization(WalletConnect client, string account, Message message)
    {
        Console.WriteLine($"Requesting WC Signature from {account}");

        var signature = await client.EthSignTypedData(account, message, NethereumSchema);

        Console.WriteLine($"Nethereum signature: {signature}");

        var recovered = _signer.RecoverFromSignatureV4(
            message, NethereumSchema, signature);

        Console.WriteLine($"Nethereum recovered: {recovered}");

        // assert the recovery matches the account
        Debug.Assert(recovered == account);
    }
}
