using WalletConnectSharp.Auth.Internals;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Tests.Common;
using WalletConnectSharp.Web3Wallet;
using Xunit;

namespace WalletConnectSharp.Auth.Tests
{
    public class AuthClientTests : IClassFixture<CryptoWalletFixture>, IAsyncLifetime
    {
        private static readonly RequestParams DefaultRequestParams = new RequestParams()
        {
            Aud = "http://localhost:3000/login",
            Domain = "localhost:3000",
            ChainId = "eip155:1",
            Nonce = CryptoUtils.GenerateNonce()
        };
        
        private readonly CryptoWalletFixture _cryptoWalletFixture;
        private WalletConnectCore _core;
        private WalletConnectAuthClient _dapp;
        private Web3WalletClient _wallet;
        

        public string WalletAddress
        {
            get
            {
                return _cryptoWalletFixture.WalletAddress;
            }
        }

        public AuthClientTests(CryptoWalletFixture cryptoWalletFixture)
        {
            this._cryptoWalletFixture = cryptoWalletFixture;
        }

        public async Task InitializeAsync()
        {
            _core = new WalletConnectCore(new CoreOptions()
            {
                ProjectId = TestValues.TestProjectId, RelayUrl = TestValues.TestRelayUrl,
            });
            _dapp = await WalletConnectAuthClient.Init(new AuthOptions()
            {
                ProjectId = TestValues.TestProjectId,
                Metadata = new AuthMetadata(),
                Name = "dapp",
            });
            _wallet = await Web3WalletClient.Init(_core, new AuthMetadata(), "wallet");
            
            Assert.NotNull(_wallet);
            Assert.NotNull(_dapp);
            Assert.NotNull(_core);
            Assert.Null(_wallet.Metadata.Redirect);
            Assert.Null(_dapp.Metadata.Redirect);
        }

        public async Task DisposeAsync()
        {
            if (_core.Relayer.Connected)
            {
                await _core.Relayer.TransportClose();
            }
        }
    }
}
