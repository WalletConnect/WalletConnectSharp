using System.Text;
using Nethereum.HdWallet;
using WalletConnectSharp.Auth.Interfaces;
using WalletConnectSharp.Auth.Internals;
using WalletConnectSharp.Auth.Models;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Publisher;
using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using ErrorResponse = WalletConnectSharp.Auth.Models.ErrorResponse;

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
        private readonly ITestOutputHelper _testOutputHelper;

        private IAuthClient PeerA;
        public IAuthClient PeerB;

        public string Iss
        {
            get
            {
                return _cryptoWalletFixture.Iss;
            }
        }

        public Wallet CryptoWallet
        {
            get
            {
                return _cryptoWalletFixture.CryptoWallet;
            }
        }

        public string WalletAddress
        {
            get
            {
                return _cryptoWalletFixture.WalletAddress;
            }
        }

        public AuthClientTests(CryptoWalletFixture cryptoFixture, ITestOutputHelper testOutputHelper)
        {
            this._cryptoWalletFixture = cryptoFixture;
            _testOutputHelper = testOutputHelper;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestInit()
        {
            Assert.NotNull(PeerA);
            Assert.NotNull(PeerB);

            Assert.NotNull(PeerA.Core);
            Assert.NotNull(PeerA.Core.Expirer);
            Assert.NotNull(PeerA.Core.History);
            Assert.NotNull(PeerA.Core.Pairing);

            Assert.NotNull(PeerB.Core);
            Assert.NotNull(PeerB.Core.Expirer);
            Assert.NotNull(PeerB.Core.History);
            Assert.NotNull(PeerB.Core.Pairing);
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestPairs()
        {
            var ogPairSize = PeerA.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> authRequested = new TaskCompletionSource<bool>();

            void OnPeerBOnAuthRequested(object o, AuthRequest authRequest) => authRequested.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            var uriData = await PeerA.Request(DefaultRequestParams);
            var uri = uriData.Uri;

            await PeerB.Core.Pairing.Pair(uri);

            await authRequested.Task;

            Assert.Equal(PeerA.Core.Pairing.Pairings.Select(p => p.Key),
                PeerB.Core.Pairing.Pairings.Select(p => p.Key));
            Assert.Equal(ogPairSize + 1, PeerA.Core.Pairing.Pairings.Length);

            var peerAHistory = await PeerA.Core.History.JsonRpcHistoryOfType<WcAuthRequest, Cacao>();
            var peerBHistory = await PeerB.Core.History.JsonRpcHistoryOfType<WcAuthRequest, Cacao>();

            Assert.Equal(peerAHistory.Size, peerBHistory.Size);

            Assert.True(PeerB.Core.Pairing.Pairings[0].Active);

            // Cleanup event listeners
            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestKnownPairings()
        {
            var ogSizeA = PeerA.Core.Pairing.Pairings.Length;
            var history = await PeerA.AuthHistory();
            var ogHistorySizeA = history.Keys.Length;

            var ogSizeB = PeerB.Core.Pairing.Pairings.Length;
            var historyB = await PeerB.AuthHistory();
            var ogHistorySizeB = historyB.Keys.Length;

            List<TopicMessage> responses = new List<TopicMessage>();
            TaskCompletionSource<TopicMessage> knownPairingTask = new TaskCompletionSource<TopicMessage>();

            async void OnPeerBOnAuthRequested(object sender, AuthRequest request)
            {
                var message = PeerB.FormatMessage(request.Parameters.CacaoPayload, this.Iss);
                var signature = await CryptoWallet.GetAccount(WalletAddress).AccountSigningService.PersonalSign
                    .SendRequestAsync(Encoding.UTF8.GetBytes(message));

                await PeerB.Respond(
                    new Cacao()
                    {
                        Id = request.Id, Signature = new Cacao.CacaoSignature.EIP191CacaoSignature(signature)
                    }, this.Iss);

                Assert.Equal(Validation.Unknown, request.VerifyContext?.Validation);
            }

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            void OnPeerAOnAuthResponded(object sender, AuthResponse args)
            {
                var sessionTopic = args.Topic;
                var cacao = args.Response.Result;
                var signature = cacao.Signature;
                _testOutputHelper.WriteLine($"{sessionTopic}: {signature}");
                responses.Add(args);
                knownPairingTask.SetResult(args);
            }

            PeerA.AuthResponded += OnPeerAOnAuthResponded;

            void OnPeerAOnAuthError(object sender, AuthErrorResponse args)
            {
                var sessionTopic = args.Topic;
                var error = args.Error;
                _testOutputHelper.WriteLine($"{sessionTopic}: {error}");
                responses.Add(args);
                knownPairingTask.SetResult(args);
            }

            PeerA.AuthError += OnPeerAOnAuthError;

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await knownPairingTask.Task;
            
            // Reset
            knownPairingTask = new TaskCompletionSource<TopicMessage>();

            // Get last pairing, that is the one we just made
            var knownPairing = PeerA.Core.Pairing.Pairings[^1];

            var requestData2 = await PeerA.Request(DefaultRequestParams, knownPairing.Topic);

            await knownPairingTask.Task;
            
            Assert.Null(requestData2.Uri);

            Assert.Equal(ogSizeA + 1, PeerA.Core.Pairing.Pairings.Length);
            Assert.Equal(ogHistorySizeA + 2, history.Keys.Length);
            Assert.Equal(ogSizeB + 1, PeerB.Core.Pairing.Pairings.Length);
            Assert.Equal(ogHistorySizeB + 2, historyB.Keys.Length);
            Assert.Equal(responses[0].Topic, responses[1].Topic);

            // Cleanup event listeners

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
            PeerA.AuthResponded -= OnPeerAOnAuthResponded;
            PeerA.AuthError -= OnPeerAOnAuthError;
        }

        [Fact, Trait("Category", "unit")]
        public async Task HandlesAuthRequests()
        {
            var ogSize = PeerB.Requests.Length;

            TaskCompletionSource<bool> receivedAuthRequest = new TaskCompletionSource<bool>();

            void OnPeerBOnAuthRequested(object o, AuthRequest authRequest) => receivedAuthRequest.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await receivedAuthRequest.Task;

            Assert.Equal(ogSize + 1, PeerB.Requests.Length);

            // Cleanup event listeners
            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestErrorResponses()
        {
            var ogPSize = PeerA.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> errorResponse = new TaskCompletionSource<bool>();

            async void OnPeerBOnAuthRequested(object sender, AuthRequest request)
            {
                await PeerB.Respond(
                    new ErrorResponse()
                    {
                        Error = new Network.Models.Error() { Code = 14001, Message = "Can not login" },
                        Id = request.Id
                    }, this.Iss);
            }

            void OnPeerAOnAuthResponded(object sender, AuthResponse response)
            {
                errorResponse.SetResult(false);
            }

            void OnPeerAOnAuthError(object sender, AuthErrorResponse response) => errorResponse.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;
            PeerA.AuthResponded += OnPeerAOnAuthResponded;
            PeerA.AuthError += OnPeerAOnAuthError;

            var requestData = await PeerA.Request(DefaultRequestParams);

            Assert.Equal(ogPSize + 1, PeerA.Core.Pairing.Pairings.Length);
            Assert.False(PeerA.Core.Pairing.Pairings[^1].Active);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await errorResponse.Task;

            Assert.False(PeerA.Core.Pairing.Pairings[^1].Active);
            Assert.True(errorResponse.Task.Result);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
            PeerA.AuthResponded -= OnPeerAOnAuthResponded;
            PeerA.AuthError -= OnPeerAOnAuthError;
        }

        [Fact, Trait("Category", "unit")]
        public async Task HandlesSuccessfulResponse()
        {
            var ogPSize = PeerA.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> successfulResponse = new TaskCompletionSource<bool>();

            async void OnPeerBOnAuthRequested(object sender, AuthRequest request)
            {
                var message = PeerB.FormatMessage(request.Parameters.CacaoPayload, this.Iss);
                var signature = await CryptoWallet.GetAccount(WalletAddress).AccountSigningService.PersonalSign
                    .SendRequestAsync(Encoding.UTF8.GetBytes(message));

                await PeerB.Respond(
                    new ResultResponse()
                    {
                        Id = request.Id, Signature = new Cacao.CacaoSignature.EIP191CacaoSignature(signature)
                    }, this.Iss);

                Assert.Equal(Validation.Unknown, request.VerifyContext?.Validation);
            }

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            void OnPeerAOnAuthResponded(object sender, AuthResponse response) =>
                successfulResponse.SetResult(response.Response.Result?.Signature != null);

            PeerA.AuthResponded += OnPeerAOnAuthResponded;

            void OnPeerAOnAuthError(object sender, AuthErrorResponse response) => successfulResponse.SetResult(false);

            PeerA.AuthError += OnPeerAOnAuthError;

            var requestData = await PeerA.Request(DefaultRequestParams);

            Assert.Equal(ogPSize + 1, PeerA.Core.Pairing.Pairings.Length);
            Assert.False(PeerA.Core.Pairing.Pairings[^1].Active);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await successfulResponse.Task;

            Assert.True(PeerA.Core.Pairing.Pairings[^1].Active);
            Assert.True(successfulResponse.Task.Result);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
            PeerA.AuthResponded -= OnPeerAOnAuthResponded;
            PeerA.AuthError -= OnPeerAOnAuthError;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestCustomRequestExpiry()
        {
            var uri = "";
            var expiry = 1000;

            TaskCompletionSource<bool> resolve1 = new TaskCompletionSource<bool>();

            EventUtils.ListenOnce<PublishParams>(
                (sender, args) =>
                {
                    Assert.Equal(expiry, args.Options.TTL);
                    resolve1.SetResult(true);
                },
                h => PeerA.Core.Relayer.Publisher.OnPublishedMessage += h,
                h => PeerA.Core.Relayer.Publisher.OnPublishedMessage -= h
            );

            await Task.WhenAll(resolve1.Task, Task.Run(async () =>
            {
                var response = await PeerA.Request(new RequestParams(DefaultRequestParams) { Expiry = expiry });
                uri = response.Uri;
            }));

            TaskCompletionSource<bool> resolve3 = new TaskCompletionSource<bool>();

            async void OnPeerBOnAuthRequested(object sender, AuthRequest request)
            {
                var message = PeerB.FormatMessage(request.Parameters.CacaoPayload, this.Iss);
                var signature = await CryptoWallet.GetAccount(WalletAddress).AccountSigningService.PersonalSign
                    .SendRequestAsync(Encoding.UTF8.GetBytes(message));

                await PeerB.Respond(
                    new ResultResponse()
                    {
                        Id = request.Id, Signature = new Cacao.CacaoSignature.EIP191CacaoSignature(signature)
                    }, this.Iss);
                resolve3.SetResult(true);
            }

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            await Task.WhenAll(resolve3.Task, PeerB.Core.Pairing.Pair(uri));

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestGetPendingPairings()
        {
            var ogCount = PeerB.PendingRequests.Count;

            TaskCompletionSource<bool> receivedAuthRequest = new TaskCompletionSource<bool>();
            var aud = "http://localhost:3000/login";

            void OnPeerBOnAuthRequested(object sender, AuthRequest request) => receivedAuthRequest.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await receivedAuthRequest.Task;

            var requests = PeerB.PendingRequests;

            Assert.Equal(ogCount + 1, requests.Count);
            Assert.Contains(requests, r => r.Value.CacaoPayload.Aud == aud);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestGetPairings()
        {
            var peerAOgSize = PeerA.Core.Pairing.Pairings.Length;
            var peerBOgSize = PeerB.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> receivedAuthRequest = new TaskCompletionSource<bool>();

            void OnPeerBOnAuthRequested(object sender, AuthRequest request) => receivedAuthRequest.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await receivedAuthRequest.Task;

            var clientPairings = PeerA.Core.Pairing.Pairings;
            var peerPairings = PeerB.Core.Pairing.Pairings;

            Assert.Equal(peerAOgSize + 1, clientPairings.Length);
            Assert.Equal(peerBOgSize + 1, peerPairings.Length);
            Assert.Equal(clientPairings[^1].Topic, peerPairings[^1].Topic);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestPing()
        {
            TaskCompletionSource<bool> receivedAuthRequest = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> receivedClientPing = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> receivedPeerPing = new TaskCompletionSource<bool>();

            void OnPeerBOnAuthRequested(object sender, AuthRequest request) => receivedAuthRequest.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            EventUtils.ListenOnce<PairingEvent>((sender, @event) =>
            {
                receivedPeerPing.SetResult(true);
            }, h => PeerB.Core.Pairing.PairingPinged += h, h => PeerB.Core.Pairing.PairingPinged -= h);

            EventUtils.ListenOnce<PairingEvent>((sender, @event) =>
            {
                receivedClientPing.SetResult(true);
            }, h => PeerA.Core.Pairing.PairingPinged += h, h => PeerA.Core.Pairing.PairingPinged -= h);

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await receivedAuthRequest.Task;

            var pairing = PeerA.Core.Pairing.Pairings[^1];
            await PeerA.Core.Pairing.Ping(pairing.Topic);
            await PeerB.Core.Pairing.Ping(pairing.Topic);

            await Task.WhenAll(receivedClientPing.Task, receivedPeerPing.Task);

            Assert.True(receivedClientPing.Task.Result);
            Assert.True(receivedPeerPing.Task.Result);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestDisconnectedPairing()
        {
            var peerAOgSize = PeerA.Core.Pairing.Pairings.Length;
            var peerBOgSize = PeerB.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> receivedAuthRequest = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> peerDeletedPairing = new TaskCompletionSource<bool>();

            void OnPeerBOnAuthRequested(object sender, AuthRequest request) => receivedAuthRequest.SetResult(true);

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            EventUtils.ListenOnce<PairingEvent>((sender, args) =>
            {
                peerDeletedPairing.SetResult(true);
            }, h => PeerB.Core.Pairing.PairingDeleted += h, h => PeerB.Core.Pairing.PairingDeleted -= h);

            var requestData = await PeerA.Request(DefaultRequestParams);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await receivedAuthRequest.Task;

            var clientPairings = PeerA.Core.Pairing.Pairings;
            var peerPairings = PeerB.Core.Pairing.Pairings;

            Assert.Equal(peerAOgSize + 1, PeerA.Core.Pairing.Pairings.Length);
            Assert.Equal(peerBOgSize + 1, PeerB.Core.Pairing.Pairings.Length);
            Assert.Equal(clientPairings[^1].Topic, peerPairings[^1].Topic);

            await PeerA.Core.Pairing.Disconnect(clientPairings[^1].Topic);

            await peerDeletedPairing.Task;
            Assert.Equal(peerAOgSize, PeerA.Core.Pairing.Pairings.Length);
            Assert.Equal(peerBOgSize, PeerB.Core.Pairing.Pairings.Length);

            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        [Fact, Trait("Category", "unit")]
        public async Task TestReceivesMetadata()
        {
            var receivedMetadataName = "";
            var ogPairingSize = PeerA.Core.Pairing.Pairings.Length;

            TaskCompletionSource<bool> hasResponded = new TaskCompletionSource<bool>();

            async void OnPeerBOnAuthRequested(object sender, AuthRequest request)
            {
                receivedMetadataName = request.Parameters.Requester?.Metadata?.Name;
                var message = PeerB.FormatMessage(request.Parameters.CacaoPayload, this.Iss);
                var signature = await CryptoWallet.GetAccount(WalletAddress).AccountSigningService.PersonalSign
                    .SendRequestAsync(Encoding.UTF8.GetBytes(message));

                await PeerB.Respond(
                    new ResultResponse()
                    {
                        Id = request.Id, Signature = new Cacao.CacaoSignature.EIP191CacaoSignature(signature)
                    }, this.Iss);

                hasResponded.SetResult(true);
                Assert.Equal(Validation.Unknown, request.VerifyContext.Validation);
            }

            PeerB.AuthRequested += OnPeerBOnAuthRequested;

            var requestData = await PeerA.Request(DefaultRequestParams);

            Assert.Equal(ogPairingSize + 1, PeerA.Core.Pairing.Pairings.Length);
            Assert.False(PeerA.Core.Pairing.Pairings[^1].Active);

            await PeerB.Core.Pairing.Pair(requestData.Uri);

            await hasResponded.Task;

            Assert.True(PeerA.Core.Pairing.Pairings[^1].Active);
            Assert.True(hasResponded.Task.Result);
            Assert.Equal(PeerA.Metadata.Name, receivedMetadataName);
            PeerB.AuthRequested -= OnPeerBOnAuthRequested;
        }

        public async Task InitializeAsync()
        {
            var OptionsA = new AuthOptions()
            {
                ProjectId = TestValues.TestProjectId,
                RelayUrl = TestValues.TestRelayUrl,
                Metadata = new Metadata()
                {
                    Description = "An example dapp to showcase WalletConnectSharpv2",
                    Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                    Name = $"WalletConnectSharpv2 Dapp Example - {Guid.NewGuid().ToString()}",
                    Url = "https://walletconnect.com"
                },
                // Omit if you want persistant storage
                Storage = new InMemoryStorage(),
            };

            var OptionsB = new AuthOptions()
            {
                ProjectId = TestValues.TestProjectId,
                RelayUrl = TestValues.TestRelayUrl,
                Metadata = new Metadata()
                {
                    Description = "An example wallet to showcase WalletConnectSharpv2",
                    Icons = new[] { "https://walletconnect.com/meta/favicon.ico" },
                    Name = $"WalletConnectSharpv2 Wallet Example - {Guid.NewGuid().ToString()}",
                    Url = "https://walletconnect.com"
                },
                // Omit if you want persistant storage
                Storage = new InMemoryStorage()
            };

            PeerA = await WalletConnectAuthClient.Init(OptionsA);
            PeerB = await WalletConnectAuthClient.Init(OptionsB);
        }

        public async Task DisposeAsync()
        {
            if (PeerA.Core.Relayer.Connected)
            {
                await PeerA.Core.Relayer.TransportClose();
            }

            if (PeerB.Core.Relayer.Connected)
            {
                await PeerB.Core.Relayer.TransportClose();
            }
        }
    }
}
