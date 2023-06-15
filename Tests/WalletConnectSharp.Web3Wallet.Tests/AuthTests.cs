using WalletConnectSharp.Auth;
using WalletConnectSharp.Auth.Internals;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Tests.Common;
using Xunit;

namespace WalletConnectSharp.Web3Wallet.Tests
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
        private string uriString;
        
        public string WalletAddress
        {
            get
            {
                return _cryptoWalletFixture.WalletAddress;
            }
        }

        public string Iss
        {
            get
            {
                return _cryptoWalletFixture.Iss;
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
                Metadata = new Metadata(),
                Name = "dapp",
            });
            _wallet = await Web3WalletClient.Init(_core, new Metadata(), "wallet");
            
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

        [Fact, Trait("Category", "unit")]
        public async void TestRespondToAuthRequest()
        {
            var request = await _dapp.Request(DefaultRequestParams);
            uriString = request.Uri;

            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.AuthRequested += async (sender, authRequest) =>
            {
                Assert.Equal(DefaultRequestParams.Aud, authRequest.Parameters.CacaoPayload.Aud);
                Assert.Equal(DefaultRequestParams.Domain, authRequest.Parameters.CacaoPayload.Domain);
                Assert.Equal(DefaultRequestParams.Nonce, authRequest.Parameters.CacaoPayload.Nonce);

                var message = _wallet.FormatMessage(authRequest.Parameters.CacaoPayload, Iss);
                var signature = await _cryptoWalletFixture.SignMessage(message);

                await _wallet.RespondAuthRequest(authRequest, signature, Iss);

                task1.TrySetResult(true);
            };

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.AuthResponded += (sender, response) =>
            {
                Assert.NotNull(response);
                Assert.NotNull(response.Id);
                Assert.NotNull(response.Topic);
                Assert.NotNull(response.Topic);
                Assert.Equal(Iss, response.Response.Result.Payload.Iss);
                task2.TrySetResult(true);
            };

            _dapp.AuthError += (sender, response) => task1.TrySetException(response.Error.ToException());

            await Task.WhenAll(
                task1.Task,
                task2.Task,
                _wallet.Pair(uriString, true)
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestShouldRejectAuthRequest()
        {
            var request = await _dapp.Request(DefaultRequestParams);
            uriString = request.Uri;
            var errorResponse = new Error()
            {
                Code = 14001,
                Message = "Can not login"
            };

            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();

            _wallet.AuthRequested += (sender, authRequest) =>
            {
                Assert.Equal(DefaultRequestParams.Aud, authRequest.Parameters.CacaoPayload.Aud);
                Assert.Equal(DefaultRequestParams.Domain, authRequest.Parameters.CacaoPayload.Domain);
                Assert.Equal(DefaultRequestParams.Nonce, authRequest.Parameters.CacaoPayload.Nonce);

                _wallet.RespondAuthRequest(authRequest, errorResponse, Iss);

                task1.TrySetResult(true);
            };

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.AuthResponded += (sender, response) =>
            {
                task2.SetException(new Exception("Did not get an error response"));
            };
            
            _dapp.AuthError += (sender, response) =>
            {
                Assert.NotNull(response);
                Assert.NotNull(response.Id);
                Assert.NotNull(response.Topic);
                Assert.Equal(errorResponse, response.Error);

                task2.TrySetResult(true);
            };

            await Task.WhenAll(
                task1.Task,
                task2.Task,
                _wallet.Pair(uriString, true)
            );
        }

        [Fact, Trait("Category", "unit")]
        public async void TestGetPendingAuthRequest()
        {
            var request = await _dapp.Request(DefaultRequestParams);
            uriString = request.Uri;

            TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
            _wallet.AuthRequested += async (sender, authRequest) =>
            {
                Assert.NotNull(authRequest.Id);
                
                var pendingRequest = _wallet.PendingAuthRequests[(long)authRequest.Id];
                
                
                Assert.Equal(DefaultRequestParams.Aud, pendingRequest.CacaoPayload.Aud);
                Assert.Equal(DefaultRequestParams.Domain, pendingRequest.CacaoPayload.Domain);
                Assert.Equal(DefaultRequestParams.Nonce, pendingRequest.CacaoPayload.Nonce);

                var message = _wallet.FormatMessage(pendingRequest.CacaoPayload, Iss);
                var signature = await _cryptoWalletFixture.SignMessage(message);

                await _wallet.RespondAuthRequest(authRequest, signature, Iss);

                task1.TrySetResult(true);
            };

            TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();
            _dapp.AuthResponded += (sender, response) =>
            {
                Assert.NotNull(response);
                Assert.NotNull(response.Id);
                Assert.NotNull(response.Topic);
                Assert.NotNull(response.Topic);
                Assert.Equal(Iss, response.Response.Result.Payload.Iss);
                task2.TrySetResult(true);
            };

            _dapp.AuthError += (sender, response) => task1.TrySetException(response.Error.ToException());
            
            await Task.WhenAll(
                task1.Task,
                task2.Task,
                _wallet.Pair(uriString, true)
            );
        }
    }
}
